using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Infrastructure.Repositories;
using Roman.RedisManager.Web.Configuration;
using System.Reflection;
using Wolverine;

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

            // Register repositories
            builder.Services.AddSingleton<IRedisServerRepository, RedisServerRepository>();

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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

                // But wait! Optimize Wolverine for usage as *only*
                // a mediator
                opts.Durability.Mode = DurabilityMode.MediatorOnly;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "v1");
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.MapControllers();

            // SPA fallback: serve index.html for non-API routes (Angular routing support)
            app.MapFallbackToFile("index.html");


            app.Run();
        }
    }
}
