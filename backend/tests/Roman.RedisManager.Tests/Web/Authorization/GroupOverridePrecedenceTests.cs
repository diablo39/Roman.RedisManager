using System.Net;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Tests.Web.Helpers;
using Roman.RedisManager.Web;
using Xunit.Abstractions;

namespace Roman.RedisManager.Tests.Web.Authorization
{
    public class GroupOverridePrecedenceTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly ITestOutputHelper _output;

        public GroupOverridePrecedenceTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
        }

        [Fact]
        public async Task DeleteKey_WithEditorRoleOnRestrictedGroup_ReturnsForbidden()
        {
            // Arrange
            var client = CreateClientWithKeyRepository(new StubRedisKeyRepository());
            TestAuthTokenFactory.ApplyBearer(client, "editor");

            // Act
            var response = await client.DeleteAsync("/api/redis-keys/test-key?groupId=33333333-3333-3333-3333-333333333333");
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
            body.ShouldNotBeNull();
        }

        [Fact]
        public async Task DeleteKey_WithAdminRoleOnRestrictedGroup_ReturnsOk()
        {
            // Arrange
            var client = CreateClientWithKeyRepository(new StubRedisKeyRepository());
            TestAuthTokenFactory.ApplyBearer(client, "admin");

            // Act
            var response = await client.DeleteAsync("/api/redis-keys/test-key?groupId=33333333-3333-3333-3333-333333333333");
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            body.ShouldContain("deleted", Case.Insensitive);
        }

        [Fact]
        public async Task DeleteKey_WhenGroupOverrideUpdatedInConfiguration_AllowsEditorWithoutCodeChange()
        {
            // Arrange
            var baselineClient = CreateClientWithKeyRepository(new StubRedisKeyRepository());
            TestAuthTokenFactory.ApplyBearer(baselineClient, "editor");

            var updatedConfiguration = new Dictionary<string, string?>
            {
                ["Redis:ServerGroups:1:AuthorizationOverrides:Rules:0:AllowedRoles:1"] = "editor"
            };

            var updatedClient = CreateClientWithKeyRepository(new StubRedisKeyRepository(), updatedConfiguration);
            TestAuthTokenFactory.ApplyBearer(updatedClient, "editor");

            // Act
            var baselineResponse = await baselineClient.DeleteAsync("/api/redis-keys/test-key?groupId=33333333-3333-3333-3333-333333333333");
            var updatedResponse = await updatedClient.DeleteAsync("/api/redis-keys/test-key?groupId=33333333-3333-3333-3333-333333333333");

            // Assert
            baselineResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
            updatedResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task DeleteKey_AuthorizationDecisionLatency_OneHundredRequestsMeetP95UnderOneSecond()
        {
            // Arrange
            var client = CreateClientWithKeyRepository(new StubRedisKeyRepository());
            TestAuthTokenFactory.ApplyBearer(client, "admin");
            var samples = new List<long>(100);

            // Act
            for (int index = 0; index < 100; index++)
            {
                var stopwatch = Stopwatch.StartNew();
                var response = await client.DeleteAsync("/api/redis-keys/test-key?groupId=33333333-3333-3333-3333-333333333333");
                stopwatch.Stop();

                response.StatusCode.ShouldBe(HttpStatusCode.OK);
                samples.Add(stopwatch.ElapsedMilliseconds);
            }

            var ordered = samples.OrderBy(value => value).ToArray();
            var p95Index = (int)Math.Ceiling(0.95 * ordered.Length) - 1;
            var p95 = ordered[Math.Max(0, p95Index)];

            // Assert
            _output.WriteLine($"Authorization decision latency validation: requests={samples.Count}, p95Ms={p95}");
            p95.ShouldBeLessThanOrEqualTo(1000);
        }

        private HttpClient CreateClientWithKeyRepository(IRedisKeyRepository repository, IDictionary<string, string?>? configurationOverrides = null)
        {
            return _factory.WithWebHostBuilder(builder =>
            {
                if (configurationOverrides is not null)
                {
                    builder.ConfigureAppConfiguration((_, configurationBuilder) =>
                    {
                        configurationBuilder.AddInMemoryCollection(configurationOverrides);
                    });
                }

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
