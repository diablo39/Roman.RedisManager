using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Web;
using Roman.RedisManager.Tests.Web.Helpers;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Entities.Server;
using Shouldly;
using System.Net.Http.Json;
using Xunit;

namespace Roman.RedisManager.Tests.Web.Controllers
{
    public class ProblemDetailsServerErrorTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ProblemDetailsServerErrorTests(WebApplicationFactory<Program> factory) => _factory = factory;

        [Fact]
        public async Task SearchKeys_RepositoryThrowsUnhandledException_Returns500ProblemDetails()
        {
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IRedisRepository));
                    if (descriptor is not null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddSingleton<IRedisRepository, ExceptionThrowingRepository>();
                });
            }).CreateClient();

            var validGuid = Guid.NewGuid();
            var response = await client.GetAsync($"/api/redis-keys?groupId={validGuid}");

            response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
            var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
            problem.ShouldBeValidProblemDetails();
        }

        private sealed class ExceptionThrowingRepository : IRedisRepository
        {
            public Task<RedisSearchResult> SearchForKeysAsync(
                Guid groupId, string pattern, string cursor, int pageSize) =>
                throw new Exception("simulated failure");

            public Task<IReadOnlyCollection<RedisServerNode>> GetServerNodesAsync(Guid groupId) =>
                throw new Exception("simulated failure");

            public Task<RedisInfo> GetInfoAsync(Guid groupId, string host, int port) =>
                throw new Exception("simulated failure");
        }
    }
}
