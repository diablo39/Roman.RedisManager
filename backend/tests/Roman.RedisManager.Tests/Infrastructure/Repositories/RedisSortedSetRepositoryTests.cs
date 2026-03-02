using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Entities.Server;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Infrastructure.Redis;
using Roman.RedisManager.Infrastructure.Repositories;
using StackExchange.Redis;

namespace Roman.RedisManager.Tests.Infrastructure.Repositories
{
    [Collection("Redis")]
    public class RedisSortedSetRepositoryTests
    {
        private readonly RedisContainerFixture _fixture;

        public RedisSortedSetRepositoryTests(RedisContainerFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task AddToSortedSetAsync_NewEntries_AddsEntries()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "zset:add:" + Guid.NewGuid();

            await repo.AddToSortedSetAsync(groupId, key, new[]
            {
                new RedisSortedSetEntry("alpha", 1.0),
                new RedisSortedSetEntry("beta", 2.5),
                new RedisSortedSetEntry("gamma", 3.0)
            }, null);

            var count = await connectionMultiplexer.GetDatabase().SortedSetLengthAsync(key);
            count.ShouldBe(3);
        }

        [Fact]
        public async Task AddToSortedSetAsync_WithTtl_SetsExpiry()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "zset:ttl:" + Guid.NewGuid();

            await repo.AddToSortedSetAsync(groupId, key, new[] { new RedisSortedSetEntry("a", 1.0) }, TimeSpan.FromSeconds(60));

            var ttl = await connectionMultiplexer.GetDatabase().KeyTimeToLiveAsync(key);
            ttl.ShouldNotBeNull();
            ttl!.Value.TotalMilliseconds.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task GetSortedSetRangeAsync_AllElements_ReturnsEntriesWithScores()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "zset:range:" + Guid.NewGuid();

            await connectionMultiplexer.GetDatabase().SortedSetAddAsync(key, new SortedSetEntry[]
            {
                new("alpha", 1.0),
                new("beta", 2.5)
            });

            var entries = await repo.GetSortedSetRangeAsync(groupId, key, 0, -1);

            entries.ShouldNotBeEmpty();
            entries.Count.ShouldBe(2);
            entries.ShouldContain(e => e.Member == "alpha" && e.Score == 1.0);
            entries.ShouldContain(e => e.Member == "beta" && e.Score == 2.5);
        }

        [Fact]
        public async Task GetSortedSetRangeAsync_NonExistentKey_ReturnsEmpty()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);

            var entries = await repo.GetSortedSetRangeAsync(groupId, "zset:missing:" + Guid.NewGuid(), 0, -1);

            entries.ShouldBeEmpty();
        }

        [Fact]
        public async Task GetSortedSetRangeAsync_ReturnsOrderedByScore()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "zset:ordered:" + Guid.NewGuid();

            await connectionMultiplexer.GetDatabase().SortedSetAddAsync(key, new SortedSetEntry[]
            {
                new("c", 3.0),
                new("a", 1.0),
                new("b", 2.0)
            });

            var entries = await repo.GetSortedSetRangeAsync(groupId, key, 0, -1);

            var ordered = entries.ToList();
            ordered[0].Member.ShouldBe("a");
            ordered[1].Member.ShouldBe("b");
            ordered[2].Member.ShouldBe("c");
        }

        [Fact]
        public async Task RemoveFromSortedSetAsync_ExistingMembers_ReturnsRemovedCount()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "zset:remove:" + Guid.NewGuid();

            await connectionMultiplexer.GetDatabase().SortedSetAddAsync(key, new SortedSetEntry[]
            {
                new("alpha", 1.0),
                new("beta", 2.0),
                new("gamma", 3.0)
            });

            var removed = await repo.RemoveFromSortedSetAsync(groupId, key, ["alpha", "beta"]);

            removed.ShouldBe(2L);
        }

        [Fact]
        public async Task RemoveFromSortedSetAsync_NonExistentMember_ReturnsZero()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "zset:remove:missing:" + Guid.NewGuid();

            await connectionMultiplexer.GetDatabase().SortedSetAddAsync(key, new SortedSetEntry[] { new("a", 1.0) });

            var removed = await repo.RemoveFromSortedSetAsync(groupId, key, ["missing"]);

            removed.ShouldBe(0L);
        }

        [Fact]
        public async Task AddToSortedSetAsync_EmptyGroupId_ThrowsArgumentException()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisSortedSetRepository repo = new RedisSortedSetRepository(connectionManager, CreateOptions());

            await Should.ThrowAsync<ArgumentException>(
                () => repo.AddToSortedSetAsync(Guid.Empty, "k", new[] { new RedisSortedSetEntry("m", 1.0) }, null));
        }

        [Fact]
        public async Task GetSortedSetRangeAsync_EmptyGroupId_ThrowsArgumentException()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisSortedSetRepository repo = new RedisSortedSetRepository(connectionManager, CreateOptions());

            await Should.ThrowAsync<ArgumentException>(
                () => repo.GetSortedSetRangeAsync(Guid.Empty, "k", 0, -1));
        }

        [Fact]
        public async Task RemoveFromSortedSetAsync_EmptyGroupId_ThrowsArgumentException()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisSortedSetRepository repo = new RedisSortedSetRepository(connectionManager, CreateOptions());

            await Should.ThrowAsync<ArgumentException>(
                () => repo.RemoveFromSortedSetAsync(Guid.Empty, "k", ["m"]));
        }

        private (IRedisSortedSetRepository repo, Guid groupId) CreateRepository(IConnectionMultiplexer connectionMultiplexer)
        {
            var options = CreateOptions();
            var connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisSortedSetRepository repo = new RedisSortedSetRepository(connectionManager, options);
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
                        Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                        Name = "sorted-set-test",
                        ConnectionString = _fixture.ConnectionString,
                        GroupType = GroupType.Standalone
                    }
                ]
            });
        }
    
}
}
