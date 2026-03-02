using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Infrastructure.Redis;
using Roman.RedisManager.Infrastructure.Repositories;
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

            builder.Services.AddSingleton<IRedisConnectionManager, RedisConnectionManager>();
            builder.Services.AddSingleton<IRedisRepository, RedisRepository>();
            builder.Services.AddSingleton<IRedisKeyRepository, RedisKeyRepository>();
            builder.Services.AddSingleton<IRedisStringRepository, RedisStringRepository>();
            builder.Services.AddSingleton<IRedisListRepository, RedisListRepository>();
            builder.Services.AddSingleton<IRedisSetRepository, RedisSetRepository>();
            builder.Services.AddSingleton<IRedisHashRepository, RedisHashRepository>();
            builder.Services.AddSingleton<IRedisSortedSetRepository, RedisSortedSetRepository>();

            builder.Services.AddControllers();
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
