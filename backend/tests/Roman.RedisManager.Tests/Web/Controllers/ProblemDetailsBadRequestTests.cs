using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Entities.Server;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Infrastructure.Exceptions;
using Shouldly;
using System.Net.Http.Json;
using Xunit;
using Roman.RedisManager.Web;
using Roman.RedisManager.Tests.Web.Helpers;

namespace Roman.RedisManager.Tests.Web.Controllers
{
    public class ProblemDetailsBadRequestTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ProblemDetailsBadRequestTests(WebApplicationFactory<Program> factory) => _factory = factory;

        [Fact]
        public async Task SearchKeys_InvalidGroupIdProducesProblemDetails400()
        {
            // Arrange
            var client = _factory.CreateClient();
            TestAuthTokenFactory.ApplyBearer(client, "redis-reader", "editor", "admin");

            // Act
            var response = await client.GetAsync("/api/redis-keys?groupId=not-a-guid");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
            problem.ShouldNotBeNull();
            problem.ShouldBeValidProblemDetails();
        }

        [Fact]
        public async Task SearchKeys_InvalidContinuationTokenProducesStableProblemDetailsCode()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IRedisRepository));
                    if (descriptor is not null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddSingleton<IRedisRepository, InvalidContinuationTokenRepository>();
                });
            }).CreateClient();
            TestAuthTokenFactory.ApplyBearer(client, "redis-reader", "editor", "admin");

            // Act
            var response = await client.GetAsync($"/api/redis-keys?groupId={Guid.NewGuid()}&continuationToken=abc");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
            problem.ShouldNotBeNull();
            problem.ShouldBeValidProblemDetails();
            problem.Extensions.ShouldContainKey("code");
            problem.Extensions["code"].ShouldNotBeNull();
            problem.Extensions["code"]!.ToString().ShouldBe("invalid_continuation_token");
        }

        private sealed class InvalidContinuationTokenRepository : IRedisRepository
        {
            public Task<RedisSearchResult> SearchForKeysAsync(Guid groupId, string pattern, string? continuationToken, int pageSize)
            {
                throw new InvalidContinuationTokenException(
                    ContinuationTokenError.InvalidContinuationToken,
                    "Continuation token is invalid. Start a new search.");
            }

            public Task<IReadOnlyCollection<RedisServerNode>> GetServerNodesAsync(Guid groupId) =>
                throw new NotImplementedException();

            public Task<RedisInfo> GetInfoAsync(Guid groupId, string host, int port) =>
                throw new NotImplementedException();
        }
    }
}
