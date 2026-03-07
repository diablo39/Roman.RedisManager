using System.Net;
using System.Text.Json;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Roman.RedisManager.Application.CQRS;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Entities.Server;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Web;
using Xunit.Abstractions;

namespace Roman.RedisManager.Tests.Web.Controllers
{
    public class RedisKeysControllerSearchTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly ITestOutputHelper _output;

        public RedisKeysControllerSearchTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
        }

        [Fact]
        public async Task SearchKeys_WithEnrichedMetadata_ReturnsConsistentKeyShape()
        {
            // Arrange
            var repository = new TrackingRedisRepository(new[]
            {
                new RedisKey("session:123", RedisDataType.String, TimeSpan.FromMinutes(10), true),
                new RedisKey("user:42:profile", RedisDataType.Hash, null, false)
            });

            var client = CreateClientWithRepository(repository, null);

            // Act
            var response = await client.GetAsync("/api/redis-keys?groupId=11111111-1111-1111-1111-111111111111&pattern=*&pageSize=10");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var payload = await response.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<RedisKeysSearchQueryResult>(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web));

            model.ShouldNotBeNull();
            model.Keys.ShouldNotBeEmpty();
            model.Keys.ShouldContain(key => key.Key == "session:123" && key.Type == "String" && key.TtlMilliseconds.HasValue && key.HasExpiration);
            model.Keys.ShouldContain(key => key.Key == "user:42:profile" && key.Type == "Hash" && key.TtlMilliseconds == null && !key.HasExpiration);
        }

        [Fact]
        public async Task SearchKeys_WithOversizedPageSize_AppliesConfiguredCap()
        {
            // Arrange
            const int configuredMaxPageSize = 7;
            var repository = new TrackingRedisRepository(Enumerable.Range(0, 100)
                .Select(index => new RedisKey($"cap:{index}", RedisDataType.String, null, false))
                .ToArray());

            var client = CreateClientWithRepository(repository, configuredMaxPageSize);

            // Act
            var response = await client.GetAsync("/api/redis-keys?groupId=11111111-1111-1111-1111-111111111111&pattern=cap:*&pageSize=999");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            repository.LastRequestedPageSize.ShouldBe(configuredMaxPageSize);

            var payload = await response.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<RedisKeysSearchQueryResult>(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            model.ShouldNotBeNull();
            model.Keys.Count.ShouldBeLessThanOrEqualTo(configuredMaxPageSize);
        }

        [Fact]
        public async Task SearchKeys_PerformanceValidation_OneHundredCappedRequestsMeetP95Target()
        {
            // Arrange
            const int configuredMaxPageSize = 25;
            var repository = new TrackingRedisRepository(Enumerable.Range(0, 500)
                .Select(index => new RedisKey($"perf:{index}", RedisDataType.String, null, false))
                .ToArray());

            var client = CreateClientWithRepository(repository, configuredMaxPageSize);
            var samples = new List<long>(100);

            // Act
            for (int i = 0; i < 100; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                var response = await client.GetAsync("/api/redis-keys?groupId=11111111-1111-1111-1111-111111111111&pattern=perf:*&pageSize=999");
                stopwatch.Stop();
                response.StatusCode.ShouldBe(HttpStatusCode.OK);
                samples.Add(stopwatch.ElapsedMilliseconds);
            }

            var ordered = samples.OrderBy(value => value).ToArray();
            var p95Index = (int)Math.Ceiling(0.95 * ordered.Length) - 1;
            var p95 = ordered[Math.Max(0, p95Index)];

            // Assert
            _output.WriteLine($"SC-003 performance validation: requests={samples.Count}, p95Ms={p95}");
            p95.ShouldBeLessThan(2000);
        }

        [Fact]
        public async Task SearchKeys_UsabilityValidation_AtLeastNinetyPercentExpirationClassificationAccuracy()
        {
            // Arrange
            var sampleKeys = new[]
            {
                new RedisKey("sample:01", RedisDataType.String, null, false),
                new RedisKey("sample:02", RedisDataType.Hash, null, false),
                new RedisKey("sample:03", RedisDataType.Set, TimeSpan.FromSeconds(30), true),
                new RedisKey("sample:04", RedisDataType.List, null, false),
                new RedisKey("sample:05", RedisDataType.String, TimeSpan.FromMinutes(1), true),
                new RedisKey("sample:06", RedisDataType.Hash, null, false),
                new RedisKey("sample:07", RedisDataType.SortedSet, TimeSpan.FromSeconds(10), true),
                new RedisKey("sample:08", RedisDataType.String, null, false),
                new RedisKey("sample:09", RedisDataType.Stream, TimeSpan.FromSeconds(45), true),
                new RedisKey("sample:10", RedisDataType.String, null, false)
            };

            var expected = sampleKeys.ToDictionary(key => key.Key, key => key.HasExpiration);
            var repository = new TrackingRedisRepository(sampleKeys);
            var client = CreateClientWithRepository(repository, null);

            // Act
            var response = await client.GetAsync("/api/redis-keys?groupId=11111111-1111-1111-1111-111111111111&pattern=sample:*&pageSize=20");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var payload = await response.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<RedisKeysSearchQueryResult>(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web));

            model.ShouldNotBeNull();
            model.Keys.Count.ShouldBe(expected.Count);

            var correct = model.Keys.Count(item => expected.TryGetValue(item.Key, out var expectedValue) && expectedValue == item.HasExpiration);
            var accuracy = (double)correct / model.Keys.Count;

            _output.WriteLine($"SC-004 usability validation: samples={model.Keys.Count}, correct={correct}, accuracy={accuracy:P2}");
            accuracy.ShouldBeGreaterThanOrEqualTo(0.9d);
        }

        private HttpClient CreateClientWithRepository(TrackingRedisRepository repository, int? overrideMaxPageSize)
        {
            var factory = _factory.WithWebHostBuilder(builder =>
            {
                if (overrideMaxPageSize.HasValue)
                {
                    builder.ConfigureAppConfiguration((_, configurationBuilder) =>
                    {
                        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            ["Redis:Search:MaxPageSize"] = overrideMaxPageSize.Value.ToString()
                        });
                    });
                }

                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(service => service.ServiceType == typeof(IRedisRepository));
                    if (descriptor is not null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddSingleton<IRedisRepository>(repository);
                });
            });

            return factory.CreateClient();
        }

        private sealed class TrackingRedisRepository : IRedisRepository
        {
            private readonly IReadOnlyCollection<RedisKey> _allKeys;

            public TrackingRedisRepository(IReadOnlyCollection<RedisKey> allKeys) => _allKeys = allKeys;

            public int LastRequestedPageSize { get; private set; }

            public Task<RedisSearchResult> SearchForKeysAsync(Guid groupId, string pattern, string? continuationToken, int pageSize)
            {
                LastRequestedPageSize = pageSize;
                var page = _allKeys.Take(pageSize).ToArray();
                return Task.FromResult(new RedisSearchResult(page, hasMoreResults: _allKeys.Count > page.Length));
            }

            public Task<IReadOnlyCollection<RedisServerNode>> GetServerNodesAsync(Guid groupId) =>
                throw new NotImplementedException();

            public Task<RedisInfo> GetInfoAsync(Guid groupId, string host, int port) =>
                throw new NotImplementedException();
        }
    }
}
