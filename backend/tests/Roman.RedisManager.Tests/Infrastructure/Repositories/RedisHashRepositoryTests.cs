using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Domain.Entities.Server;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Infrastructure.Redis;
using Roman.RedisManager.Infrastructure.Repositories;
using StackExchange.Redis;

namespace Roman.RedisManager.Tests.Infrastructure.Repositories
{
    [Collection("Redis")]
    public class RedisHashRepositoryTests
    {
        private readonly RedisContainerFixture _fixture;

        public RedisHashRepositoryTests(RedisContainerFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task SetHashFieldsAsync_NewFields_SetsFields()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "hash:set:" + Guid.NewGuid();

            await repo.SetHashFieldsAsync(groupId, key, new Dictionary<string, string> { ["name"] = "Alice", ["age"] = "30" }, null);

            var count = await connectionMultiplexer.GetDatabase().HashLengthAsync(key);
            count.ShouldBe(2);
        }

        [Fact]
        public async Task SetHashFieldsAsync_WithTtl_SetsExpiry()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "hash:ttl:" + Guid.NewGuid();

            await repo.SetHashFieldsAsync(groupId, key, new Dictionary<string, string> { ["f"] = "v" }, TimeSpan.FromSeconds(60));

            var ttl = await connectionMultiplexer.GetDatabase().KeyTimeToLiveAsync(key);
            ttl.ShouldNotBeNull();
            ttl!.Value.TotalMilliseconds.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task HashScanAsync_ExistingHash_ReturnsFields()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "hash:scan:" + Guid.NewGuid();

            await connectionMultiplexer.GetDatabase().HashSetAsync(key, new HashEntry[]
            {
                new("name", "Bob"),
                new("city", "Warsaw")
            });

            var result = await repo.HashScanAsync(groupId, key, 0, 100);

            result.ShouldNotBeNull();
            result.Items.ShouldNotBeEmpty();
            result.Items.ShouldContain(e => e.Field == "name" && e.Value == "Bob");
            result.Items.ShouldContain(e => e.Field == "city" && e.Value == "Warsaw");
        }

        [Fact]
        public async Task HashScanAsync_NonExistentKey_ReturnsEmpty()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);

            var result = await repo.HashScanAsync(groupId, "hash:missing:" + Guid.NewGuid(), 0, 100);

            result.ShouldNotBeNull();
            result.Items.ShouldBeEmpty();
        }

        [Fact]
        public async Task RemoveHashFieldsAsync_ExistingFields_ReturnsRemovedCount()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "hash:remove:" + Guid.NewGuid();

            await connectionMultiplexer.GetDatabase().HashSetAsync(key, new HashEntry[]
            {
                new("name", "Alice"),
                new("age", "30"),
                new("city", "London")
            });

            var removed = await repo.RemoveHashFieldsAsync(groupId, key, ["name", "age"]);

            removed.ShouldBe(2L);
        }

        [Fact]
        public async Task RemoveHashFieldsAsync_NonExistentField_ReturnsZero()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "hash:remove:missing:" + Guid.NewGuid();

            await connectionMultiplexer.GetDatabase().HashSetAsync(key, new HashEntry[] { new("f", "v") });

            var removed = await repo.RemoveHashFieldsAsync(groupId, key, ["nonexistent"]);

            removed.ShouldBe(0L);
        }

        [Fact]
        public async Task SetHashFieldsAsync_EmptyGroupId_ThrowsArgumentException()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            await using IRedisConnectionManager connectionManager = new SimpleConnectionManager(connectionMultiplexer);
            IRedisHashRepository repo = new RedisHashRepository(connectionManager, CreateOptions());

            await Should.ThrowAsync<ArgumentException>(
                () => repo.SetHashFieldsAsync(Guid.Empty, "k", new Dictionary<string, string> { ["f"] = "v" }, null));
        }

        [Fact]
        public async Task HashScanAsync_EmptyGroupId_ThrowsArgumentException()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            await using IRedisConnectionManager connectionManager = new SimpleConnectionManager(connectionMultiplexer);
            IRedisHashRepository repo = new RedisHashRepository(connectionManager, CreateOptions());

            await Should.ThrowAsync<ArgumentException>(
                () => repo.HashScanAsync(Guid.Empty, "k", 0, 100));
        }

        private (IRedisHashRepository repo, Guid groupId) CreateRepository(IConnectionMultiplexer connectionMultiplexer)
        {
            var options = CreateOptions();
            var connectionManager = new SimpleConnectionManager(connectionMultiplexer);
            IRedisHashRepository repo = new RedisHashRepository(connectionManager, options);
            var groupId = options.Value.ServerGroups.First().Id;
            return (repo, groupId);
        }

        private IOptions<RedisConfiguration> CreateOptions()
        {
            return Options.Create(new RedisConfiguration
            {
                ServerGroups =
                [
                    new RedisServerGroupConfiguration
                    {
                        Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                        Name = "hash-test",
                        ConnectionString = _fixture.ConnectionString,
                        GroupType = GroupType.Standalone
                    }
                ]
            });
        }

        private sealed class SimpleConnectionManager : IRedisConnectionManager
        {
            private readonly IConnectionMultiplexer _multiplexer;

            public SimpleConnectionManager(IConnectionMultiplexer multiplexer) => _multiplexer = multiplexer;

            public Task<IConnectionMultiplexer> GetConnectionAsync(Guid groupId) => Task.FromResult(_multiplexer);

            public ValueTask DisposeAsync()
            {
                _multiplexer.Dispose();
                return ValueTask.CompletedTask;
            }
        }
    }
}
