using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Entities.Server;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Infrastructure.Exceptions;
using Roman.RedisManager.Infrastructure.Redis;
using Roman.RedisManager.Infrastructure.Repositories;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;

namespace Roman.RedisManager.Tests.Infrastructure.Repositories
{
    [Collection("Redis")]
    public class RedisRepositoryTests
    {
        private readonly RedisContainerFixture _fixture;

        public RedisRepositoryTests(RedisContainerFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task SearchForKeysAsync_WithSeededKey_ReturnsNonEmptyResult()
        {
            using var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            await connectionMultiplexer.GetDatabase().StringSetAsync("seed", "value");

            var options = CreateRedisOptions(GroupType.Standalone, Guid.Parse("33333333-3333-3333-3333-333333333333"));
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisRepository repository = CreateRepository(connectionManager, options);

            var result = await repository.SearchForKeysAsync(options.Value.ServerGroups.First().Id, "*", null, 50);

            result.ShouldNotBeNull();
            result.Keys.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task SearchForKeysAsync_WithContinuationToken_ReplaysNextPage()
        {
            using var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var db = connectionMultiplexer.GetDatabase();
            for (int i = 0; i < 120; i++)
            {
                await db.StringSetAsync($"cursor-chain:{i}", "v");
            }

            var options = CreateRedisOptions(GroupType.Standalone, Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisRepository repository = CreateRepository(connectionManager, options);
            var groupId = options.Value.ServerGroups.First().Id;

            var page1 = await repository.SearchForKeysAsync(groupId, "cursor-chain:*", null, 5);

            page1.HasMoreResults.ShouldBeTrue();
            page1.ContinuationToken.ShouldNotBeNullOrWhiteSpace();

            var page2 = await repository.SearchForKeysAsync(groupId, "cursor-chain:*", page1.ContinuationToken, 5);
            page2.ShouldNotBeNull();
            page2.Keys.Count().ShouldBeLessThanOrEqualTo(5);
        }

        [Fact]
        public async Task SearchForKeysAsync_StandaloneContinuation_ReplaysThreeConsecutivePages()
        {
            using var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var db = connectionMultiplexer.GetDatabase();
            for (int i = 0; i < 180; i++)
            {
                await db.StringSetAsync($"standalone-3pages:{i}", "v");
            }

            var options = CreateRedisOptions(GroupType.Standalone, Guid.Parse("92929292-9292-9292-9292-929292929292"));
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisRepository repository = CreateRepository(connectionManager, options);
            var groupId = options.Value.ServerGroups.First().Id;

            var page1 = await repository.SearchForKeysAsync(groupId, "standalone-3pages:*", null, 5);
            page1.Keys.Count().ShouldBeLessThanOrEqualTo(5);
            page1.HasMoreResults.ShouldBeTrue();

            var page2 = await repository.SearchForKeysAsync(groupId, "standalone-3pages:*", page1.ContinuationToken, 5);
            page2.Keys.Count().ShouldBeLessThanOrEqualTo(5);
            page2.HasMoreResults.ShouldBeTrue();

            var page3 = await repository.SearchForKeysAsync(groupId, "standalone-3pages:*", page2.ContinuationToken, 5);
            page3.Keys.Count().ShouldBeLessThanOrEqualTo(5);
        }

        [Fact]
        public async Task SearchForKeysAsync_ClusterContinuation_ReplaysThreeConsecutivePages()
        {
            using var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var db = connectionMultiplexer.GetDatabase();
            for (int i = 0; i < 180; i++)
            {
                await db.StringSetAsync($"cluster-3pages:{i}", "v");
            }

            var options = CreateRedisOptions(GroupType.Cluster, Guid.Parse("93939393-9393-9393-9393-939393939393"));
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisRepository repository = CreateRepository(connectionManager, options);
            var groupId = options.Value.ServerGroups.First().Id;

            var page1 = await repository.SearchForKeysAsync(groupId, "cluster-3pages:*", null, 5);
            page1.Keys.Count().ShouldBeLessThanOrEqualTo(5);
            page1.HasMoreResults.ShouldBeTrue();
            page1.ContinuationToken.ShouldNotBeNullOrWhiteSpace();

            var page2 = await repository.SearchForKeysAsync(groupId, "cluster-3pages:*", page1.ContinuationToken, 5);
            page2.Keys.Count().ShouldBeLessThanOrEqualTo(5);
            page2.HasMoreResults.ShouldBeTrue();
            page2.ContinuationToken.ShouldNotBeNullOrWhiteSpace();

            var page3 = await repository.SearchForKeysAsync(groupId, "cluster-3pages:*", page2.ContinuationToken, 5);
            page3.Keys.Count().ShouldBeLessThanOrEqualTo(5);
        }

        [Fact]
        public async Task SearchForKeysAsync_WithMalformedContinuationToken_ThrowsInvalidContinuationToken()
        {
            using var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var options = CreateRedisOptions(GroupType.Standalone, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisRepository repository = CreateRepository(connectionManager, options);
            var groupId = options.Value.ServerGroups.First().Id;

            var ex = await Should.ThrowAsync<InvalidContinuationTokenException>(() =>
                repository.SearchForKeysAsync(groupId, "*", "invalid-token", 50));

            ex.ErrorCode.ShouldBe(ContinuationTokenError.InvalidContinuationToken);
        }

        [Fact]
        public async Task SearchForKeysAsync_WithMismatchedPattern_ThrowsContextMismatch()
        {
            using var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var db = connectionMultiplexer.GetDatabase();
            await db.StringSetAsync("mismatch:1", "v");

            var options = CreateRedisOptions(GroupType.Standalone, Guid.Parse("12121212-1212-1212-1212-121212121212"));
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisRepository repository = CreateRepository(connectionManager, options);
            var groupId = options.Value.ServerGroups.First().Id;

            var page1 = await repository.SearchForKeysAsync(groupId, "mismatch:*", null, 2);
            page1.ContinuationToken.ShouldNotBeNullOrWhiteSpace();

            var ex = await Should.ThrowAsync<InvalidContinuationTokenException>(() =>
                repository.SearchForKeysAsync(groupId, "different:*", page1.ContinuationToken, 2));

            ex.ErrorCode.ShouldBe(ContinuationTokenError.ContinuationContextMismatch);
        }

        [Fact]
        public async Task SearchForKeysAsync_WithExpiredToken_ThrowsNotResumable()
        {
            using var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var options = CreateRedisOptions(GroupType.Standalone, Guid.Parse("56565656-5656-5656-5656-565656565656"));
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisRepository repository = CreateRepository(connectionManager, options);
            var groupId = options.Value.ServerGroups.First().Id;

            var codec = CreateCodec();
            var expiredEnvelope = new ContinuationTokenEnvelope
            {
                Version = 1,
                ContextHash = ComputeContextHash(groupId, "*", 50),
                Mode = ContinuationTokenMode.Standalone,
                IssuedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-120),
                StandaloneState = new StandaloneCursorState(42)
            };
            var token = codec.Encode(expiredEnvelope);

            var ex = await Should.ThrowAsync<InvalidContinuationTokenException>(() =>
                repository.SearchForKeysAsync(groupId, "*", token, 50));

            ex.ErrorCode.ShouldBe(ContinuationTokenError.ContinuationNotResumable);
        }

        [Fact]
        public async Task SearchForKeysAsync_WithSmallPageSize_ReturnsBoundedPage()
        {
            using var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var db = connectionMultiplexer.GetDatabase();
            for (int i = 0; i < 20; i++)
            {
                await db.StringSetAsync($"paging-test:{i}", "v");
            }

            var options = CreateRedisOptions(GroupType.Standalone, Guid.Parse("77777777-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisRepository repository = CreateRepository(connectionManager, options);
            var groupId = options.Value.ServerGroups.First().Id;

            var result = await repository.SearchForKeysAsync(groupId, "paging-test:*", null, 5);

            result.Keys.Count().ShouldBeLessThanOrEqualTo(5);
        }

        [Fact]
        public async Task SearchForKeysAsync_FinalPage_HasNoContinuationToken()
        {
            using var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var options = CreateRedisOptions(GroupType.Standalone, Guid.Parse("98989898-9898-9898-9898-989898989898"));
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisRepository repository = CreateRepository(connectionManager, options);
            var groupId = options.Value.ServerGroups.First().Id;

            var result = await repository.SearchForKeysAsync(groupId, "no-match:pattern:*", null, 50);

            result.HasMoreResults.ShouldBeFalse();
            result.ContinuationToken.ShouldBeNull();
        }

        [Fact]
        public async Task SearchForKeysAsync_ClusterGroup_UsesUnifiedContinuationTokenContract()
        {
            using var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var db = connectionMultiplexer.GetDatabase();
            for (int i = 0; i < 120; i++)
            {
                await db.StringSetAsync($"cluster-token:{i}", "v");
            }

            var options = CreateRedisOptions(GroupType.Cluster, Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"));
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisRepository repository = CreateRepository(connectionManager, options);
            var groupId = options.Value.ServerGroups.First().Id;

            var page1 = await repository.SearchForKeysAsync(groupId, "cluster-token:*", null, 5);

            page1.ShouldNotBeNull();
            if (page1.HasMoreResults)
            {
                page1.ContinuationToken.ShouldNotBeNullOrWhiteSpace();
                var page2 = await repository.SearchForKeysAsync(groupId, "cluster-token:*", page1.ContinuationToken, 5);
                page2.ShouldNotBeNull();
            }
        }

        [Fact]
        public async Task GetServerNodesAsync_WithValidGroupId_ReturnsAtLeastOneNode()
        {
            using var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var options = CreateRedisOptions(GroupType.Standalone, Guid.Parse("66666666-6666-6666-6666-666666666666"));
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisRepository repository = CreateRepository(connectionManager, options);
            var groupId = options.Value.ServerGroups.First().Id;

            var nodes = await repository.GetServerNodesAsync(groupId);

            nodes.ShouldNotBeNull();
            nodes.ShouldNotBeEmpty();
            nodes.All(n => n.Role == "master" || n.Role == "slave").ShouldBeTrue();
        }

        [Fact]
        public async Task GetServerNodesAsync_InvalidGroupId_ThrowsKeyNotFoundException()
        {
            using var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var options = CreateRedisOptions(GroupType.Standalone, Guid.Parse("78787878-7878-7878-7878-787878787878"));
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisRepository repository = CreateRepository(connectionManager, options);

            await Should.ThrowAsync<KeyNotFoundException>(() => repository.GetServerNodesAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetInfoAsync_WithValidGroupId_ReturnsStructuredSections()
        {
            using var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var options = CreateRedisOptions(GroupType.Standalone, Guid.Parse("44444444-4444-4444-4444-444444444444"));
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisRepository repository = CreateRepository(connectionManager, options);
            var groupId = options.Value.ServerGroups.First().Id;

            var endpointParts = _fixture.Container.GetConnectionString().Split(':');
            var info = await repository.GetInfoAsync(groupId, endpointParts[0], int.Parse(endpointParts[1]));

            info.ShouldNotBeNull();
            info.Sections.ShouldNotBeEmpty();
        }

        private IRedisRepository CreateRepository(
            IRedisConnectionManager connectionManager,
            IOptions<RedisConfiguration> redisOptions)
        {
            return new RedisRepository(
                connectionManager,
                redisOptions,
                CreateCodec(),
                CreateTokenOptions());
        }

        private static ContinuationTokenCodec CreateCodec() => new(CreateTokenOptions());

        private static IOptions<ContinuationTokenConfiguration> CreateTokenOptions()
        {
            return Options.Create(new ContinuationTokenConfiguration
            {
                TokenSecret = "test-secret-key-with-sufficient-length",
                TokenTtlMinutes = 60
            });
        }

        private IOptions<RedisConfiguration> CreateRedisOptions(GroupType groupType, Guid groupId)
        {
            return Options.Create(new RedisConfiguration
            {
                ServerGroups =
                [
                    new RedisServerGroupConfiguration
                    {
                        Id = groupId,
                        Name = "test-group",
                        ConnectionString = _fixture.Container.GetConnectionString(),
                        GroupType = groupType
                    }
                ]
            });
        }

        private static string ComputeContextHash(Guid groupId, string pattern, int pageSize)
        {
            var payload = $"{groupId:N}|{pattern}|{pageSize}";
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
            return Convert.ToHexString(hash);
        }
    }
}
