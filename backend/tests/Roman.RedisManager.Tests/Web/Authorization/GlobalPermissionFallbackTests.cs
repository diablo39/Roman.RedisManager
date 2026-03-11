using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Tests.Web.Helpers;
using Roman.RedisManager.Web;

namespace Roman.RedisManager.Tests.Web.Authorization
{
    public class GlobalPermissionFallbackTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public GlobalPermissionFallbackTests(WebApplicationFactory<Program> factory) => _factory = factory;

        [Fact]
        public async Task DeleteKey_WithEditorRoleOnNonOverriddenGroup_ReturnsOk()
        {
            // Arrange
            var client = CreateClientWithKeyRepository(new StubRedisKeyRepository());
            TestAuthTokenFactory.ApplyBearer(client, "editor");

            // Act
            var response = await client.DeleteAsync("/api/redis-keys/test-key?groupId=22222222-2222-2222-2222-222222222222");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        private HttpClient CreateClientWithKeyRepository(IRedisKeyRepository repository)
        {
            return _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(service => service.ServiceType == typeof(IRedisKeyRepository));
                    if (descriptor is not null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddSingleton(repository);
                });
            }).CreateClient();
        }

        private sealed class StubRedisKeyRepository : IRedisKeyRepository
        {
            public Task<bool> DeleteKeyAsync(Guid groupId, string key) => Task.FromResult(true);

            public Task<RedisKeyMetadata> GetKeyMetadataAsync(Guid groupId, string key) =>
                throw new NotImplementedException();

            public Task<RedisKeyValue> GetKeyValueAsync(Guid groupId, string key) =>
                throw new NotImplementedException();
        }
    }
}
