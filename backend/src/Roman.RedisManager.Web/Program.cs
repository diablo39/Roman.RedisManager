using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Domain.Repositories.RedisDataTypes;
using Roman.RedisManager.Infrastructure.Redis;
using Roman.RedisManager.Infrastructure.Exceptions;
using Roman.RedisManager.Infrastructure.Repositories;
using Roman.RedisManager.Infrastructure.Repositories.RedisDataTypes;
using Roman.RedisManager.Web.Authentication;
using Roman.RedisManager.Web.Authorization;
using Wolverine;
using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authentication;
// using Microsoft.AspNetCore.Mvc.Infrastructure; // no longer needed

namespace Roman.RedisManager.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services
                .AddOptions<RedisConfiguration>()
                .Bind(builder.Configuration.GetSection(RedisConfiguration.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services
                .AddOptions<RedisSearchLimitsConfiguration>()
                .Bind(builder.Configuration.GetSection(RedisConfiguration.SectionName).GetSection(RedisSearchLimitsConfiguration.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services
                .AddOptions<ContinuationTokenConfiguration>()
                .Bind(builder.Configuration.GetSection(ContinuationTokenConfiguration.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services
                .AddOptions<OidcAuthenticationConfiguration>()
                .Bind(builder.Configuration.GetSection(OidcAuthenticationConfiguration.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services
                .AddOptions<AuthorizationRoleMappingConfiguration>()
                .Bind(builder.Configuration.GetSection(AuthorizationRoleMappingConfiguration.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services.AddSingleton<IRedisConnectionManager, RedisConnectionManager>();
            builder.Services.AddSingleton<IContinuationTokenCodec, ContinuationTokenCodec>();
            builder.Services.AddSingleton<IRedisRepository, RedisRepository>();
            builder.Services.AddSingleton<IRedisKeyRepository, RedisKeyRepository>();
            builder.Services.AddSingleton<IRedisStringRepository, RedisStringRepository>();
            builder.Services.AddSingleton<IRedisListRepository, RedisListRepository>();
            builder.Services.AddSingleton<IRedisSetRepository, RedisSetRepository>();
            builder.Services.AddSingleton<IRedisHashRepository, RedisHashRepository>();
            builder.Services.AddSingleton<IRedisSortedSetRepository, RedisSortedSetRepository>();
            builder.Services.AddSingleton<AuthorizationDecisionLogger>();
            builder.Services.AddSingleton<RoleClaimMappingEvaluator>();

            builder.Services.AddConfiguredIdentity(builder.Configuration);

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthorizationPolicies.Reader, policy => policy
                    .RequireAuthenticatedUser()
                    .RequireRole("reader", "admin"));

                options.AddPolicy(AuthorizationPolicies.Editor, policy => policy
                    .RequireAuthenticatedUser()
                    .RequireRole("editor", "admin"));

                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationResultHandler>();
            builder.Services.AddTransient<IClaimsTransformation, NormalizedRoleClaimsTransformation>();

            builder.Services.AddControllers();
            builder.Services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = problemContext =>
                {
                    var context = problemContext.HttpContext;
                    var details = problemContext.ProblemDetails;

                    // continuation-token specific handling moved to controller; keep only generic extensions here
                    var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
                    details.Extensions["traceId"] = traceId;

                    if (context.Request.Headers.TryGetValue("traceparent", out var tp))
                    {
                        details.Extensions["traceparent"] = tp.ToString();
                    }

                    details.Extensions["requestPath"] = context.Request.Path.ToString();
                };
            });
            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer<OpenApi.SecuritySchemeTransformer>();
                options.AddOperationTransformer<OpenApi.SecurityRequirementOperationTransformer>();
            });

            // Add permissive CORS policy for Development and Testing environments to simplify SPA testing.
            if (builder.Environment.IsDevelopment() || string.Equals(builder.Environment.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase))
            {
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("PermissiveDevCors", policy =>
                    {
                        policy.SetIsOriginAllowed(_ => true)
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    });
                });
            }

            builder.UseWolverine(opts =>
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetName().Name!.StartsWith("Roman.RedisManager"))
                    {
                        opts.Discovery.IncludeAssembly(assembly);
                    }
                }

                opts.Durability.Mode = DurabilityMode.MediatorOnly;
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi().AllowAnonymous();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "v1");
                });
            }

            // Enable permissive CORS in Development and Testing to allow local SPA testing.
            if (app.Environment.IsDevelopment() || string.Equals(app.Environment.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase))
            {
                app.UseCors("PermissiveDevCors");
            }

            // Problem details middleware: exception handler and status code pages
            app.UseExceptionHandler();
            app.UseStatusCodePages();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.MapControllers();

            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}
