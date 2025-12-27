
using Roman.RedisManager.Web.Configuration;

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

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
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
