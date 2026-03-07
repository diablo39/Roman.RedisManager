using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Domain.Repositories.RedisDataTypes;
using Roman.RedisManager.Infrastructure.Redis;
using Roman.RedisManager.Infrastructure.Exceptions;
using Roman.RedisManager.Infrastructure.Repositories;
using Roman.RedisManager.Infrastructure.Repositories.RedisDataTypes;
using Roman.RedisManager.Web.ProblemDetails;
using Wolverine;
using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
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
                .AddOptions<ContinuationTokenConfiguration>()
                .Bind(builder.Configuration.GetSection(ContinuationTokenConfiguration.SectionName))
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

            builder.Services.AddControllers();
            builder.Services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = problemContext =>
                {
                    var context = problemContext.HttpContext;
                    var details = problemContext.ProblemDetails;
                    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

                    if (exception is InvalidContinuationTokenException continuationException)
                    {
                        details.Status = StatusCodes.Status400BadRequest;
                        details.Title = "Invalid continuation token";
                        details.Detail = continuationException.Message;
                        details.Type = ContinuationTokenProblemDetailsMapper.ToType(continuationException.ErrorCode);
                        details.Extensions["code"] = ContinuationTokenProblemDetailsMapper.ToCode(continuationException.ErrorCode);

                        var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
                        var logger = loggerFactory.CreateLogger("ContinuationTokenProblemDetails");
                        logger.LogWarning(
                            "Rejected continuation token with error code {ErrorCode} on {RequestPath}",
                            ContinuationTokenProblemDetailsMapper.ToCode(continuationException.ErrorCode),
                            context.Request.Path.ToString());
                    }

                    var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
                    details.Extensions["traceId"] = traceId;

                    if (context.Request.Headers.TryGetValue("traceparent", out var tp))
                    {
                        details.Extensions["traceparent"] = tp.ToString();
                    }

                    details.Extensions["requestPath"] = context.Request.Path.ToString();
                };
            });
            builder.Services.AddOpenApi();

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
                app.MapOpenApi();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "v1");
                });
            }

            // Problem details middleware: exception handler and status code pages
            app.UseExceptionHandler();
            app.UseStatusCodePages();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.MapControllers();

            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}
