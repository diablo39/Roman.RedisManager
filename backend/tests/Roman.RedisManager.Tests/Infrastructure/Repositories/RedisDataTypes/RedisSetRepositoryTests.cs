using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Domain.Entities.Server;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Infrastructure.Redis;
using Roman.RedisManager.Infrastructure.Repositories.RedisDataTypes;
using StackExchange.Redis;

namespace Roman.RedisManager.Tests.Infrastructure.Repositories.RedisDataTypes
{
    [Collection("Redis")]
    public class RedisSetRepositoryTests
    {
        private readonly RedisContainerFixture _fixture;

        public RedisSetRepositoryTests(RedisContainerFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task SetAddAsync_NewMembers_AddsMembersToSet()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "set:add:" + Guid.NewGuid();

            await repo.SetAddAsync(groupId, key, ["x", "y", "z"], null);

            var count = await connectionMultiplexer.GetDatabase().SetLengthAsync(key);
            count.ShouldBe(3);
        }

        [Fact]
        public async Task SetAddAsync_WithTtl_SetsExpiry()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "set:ttl:" + Guid.NewGuid();

            await repo.SetAddAsync(groupId, key, ["a"], TimeSpan.FromSeconds(60));

            var ttl = await connectionMultiplexer.GetDatabase().KeyTimeToLiveAsync(key);
            ttl.ShouldNotBeNull();
            ttl!.Value.TotalMilliseconds.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task SetAddAsync_DuplicateMembers_StoresUnique()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "set:unique:" + Guid.NewGuid();

            await repo.SetAddAsync(groupId, key, ["a", "a", "b"], null);

            var count = await connectionMultiplexer.GetDatabase().SetLengthAsync(key);
            count.ShouldBe(2);
        }

        [Fact]
        public async Task SetScanAsync_ExistingSet_ReturnsMembers()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "set:scan:" + Guid.NewGuid();

            await connectionMultiplexer.GetDatabase().SetAddAsync(key, new RedisValue[] { "x", "y", "z" });

            var result = await repo.SetScanAsync(groupId, key, 0, 100);

            result.ShouldNotBeNull();
            result.Items.ShouldNotBeEmpty();
            result.Items.ShouldContain("x");
            result.Items.ShouldContain("y");
            result.Items.ShouldContain("z");
            result.HasMoreResults.ShouldBeFalse();
        }

        [Fact]
        public async Task SetScanAsync_NonExistentKey_ReturnsEmpty()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);

            var result = await repo.SetScanAsync(groupId, "set:missing:" + Guid.NewGuid(), 0, 100);

            result.ShouldNotBeNull();
            result.Items.ShouldBeEmpty();
            result.HasMoreResults.ShouldBeFalse();
        }

        [Fact]
        public async Task SetRemoveAsync_ExistingMembers_ReturnsRemovedCount()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "set:remove:" + Guid.NewGuid();

            await connectionMultiplexer.GetDatabase().SetAddAsync(key, new RedisValue[] { "a", "b", "c" });

            var removed = await repo.SetRemoveAsync(groupId, key, ["a", "b"]);

            removed.ShouldBe(2L);
        }

        [Fact]
        public async Task SetRemoveAsync_NonExistentMember_ReturnsZero()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "set:remove:missing:" + Guid.NewGuid();

            await connectionMultiplexer.GetDatabase().SetAddAsync(key, new RedisValue[] { "a" });

            var removed = await repo.SetRemoveAsync(groupId, key, ["z"]);

            removed.ShouldBe(0L);
        }

        [Fact]
        public async Task SetAddAsync_EmptyGroupId_ThrowsArgumentException()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisSetRepository repo = new RedisSetRepository(connectionManager, CreateOptions());

            await Should.ThrowAsync<ArgumentException>(
                () => repo.SetAddAsync(Guid.Empty, "k", ["m"], null));
        }

        [Fact]
        public async Task SetScanAsync_EmptyGroupId_ThrowsArgumentException()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisSetRepository repo = new RedisSetRepository(connectionManager, CreateOptions());

            await Should.ThrowAsync<ArgumentException>(
                () => repo.SetScanAsync(Guid.Empty, "k", 0, 100));
        }

        [Fact]
        public async Task SetRemoveAsync_EmptyGroupId_ThrowsArgumentException()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisSetRepository repo = new RedisSetRepository(connectionManager, CreateOptions());

            await Should.ThrowAsync<ArgumentException>(
                () => repo.SetRemoveAsync(Guid.Empty, "k", ["m"]));
        }

        private (IRedisSetRepository repo, Guid groupId) CreateRepository(IConnectionMultiplexer connectionMultiplexer)
        {
            var options = CreateOptions();
            var connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisSetRepository repo = new RedisSetRepository(connectionManager, options);
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
                        Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                        Name = "set-test",
                        ConnectionString = _fixture.ConnectionString,
                        GroupType = GroupType.Standalone
                    }
                ]
            });
        }
    
}
}
