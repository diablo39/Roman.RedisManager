using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Entities.Server;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Infrastructure.Exceptions;
using Roman.RedisManager.Web;

namespace Roman.RedisManager.Tests.Web.Controllers
{
    public class RedisServerGroupsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public RedisServerGroupsControllerTests(WebApplicationFactory<Program> factory) => _factory = factory;

        [Fact]
        public async Task GetServerGroups_WithConfiguredGroups_Returns200WithGroups()
        {


            // Arrange

            // Act
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/redis-server-groups");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var body = await response.Content.ReadAsStringAsync();
            body.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetServerGroupDetail_UnknownGroupId_Returns404()
        {


            // Arrange

            // Act
            var client = _factory.CreateClient();
            var unknownId = Guid.NewGuid();

            var response = await client.GetAsync($"/api/redis-server-groups/{unknownId}");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetServerGroupDetail_RedisConnectionFailure_Returns500()
        {

            // Arrange
            // Override IRedisRepository with a stub that throws RedisConnectionFailureException
            // to simulate a Redis connectivity problem without needing a real broken server.
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the real IRedisRepository and replace with one that always
                    // throws RedisConnectionFailureException on GetServerNodesAsync.
                    var descriptor = services.SingleOrDefault(

                        d => d.ServiceType == typeof(IRedisRepository));

            // Act

            // Assert
                    if (descriptor is not null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddSingleton<IRedisRepository, AlwaysFailingRedisRepository>();
                });
            }).CreateClient();

            // Use a known group ID from appsettings.json ("11111111-1111-1111-1111-111111111111")
            var response = await client.GetAsync("/api/redis-server-groups/11111111-1111-1111-1111-111111111111");

            response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        }

        private sealed class AlwaysFailingRedisRepository : IRedisRepository
        {
            public Task<RedisSearchResult> SearchForKeysAsync(
                Guid groupId, string pattern, string? continuationToken, int pageSize) =>
                throw new RedisConnectionFailureException(
                    "Simulated connection failure.", new Exception());

            public Task<IReadOnlyCollection<RedisServerNode>> GetServerNodesAsync(Guid groupId) =>
                throw new RedisConnectionFailureException(
                    "Simulated connection failure.", new Exception());

            public Task<RedisInfo> GetInfoAsync(Guid groupId, string host, int port) =>
                throw new RedisConnectionFailureException(
                    "Simulated connection failure.", new Exception());
        }
    }
}
