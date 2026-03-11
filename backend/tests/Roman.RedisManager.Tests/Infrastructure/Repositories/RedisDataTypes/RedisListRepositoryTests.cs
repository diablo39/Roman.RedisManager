using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Entities.Server;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Domain.Repositories.RedisDataTypes;
using Roman.RedisManager.Infrastructure.Redis;
using Roman.RedisManager.Infrastructure.Repositories.RedisDataTypes;
using StackExchange.Redis;

namespace Roman.RedisManager.Tests.Infrastructure.Repositories.RedisDataTypes
{
    [Collection("Redis")]
    public class RedisListRepositoryTests
    {
        private readonly RedisContainerFixture _fixture;

        public RedisListRepositoryTests(RedisContainerFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task ListPushAsync_RightDirection_AddsValues()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "list:push:right:" + Guid.NewGuid();

            // Act
            await repo.ListPushAsync(groupId, key, ["a", "b", "c"], ListDirection.Right, null);

            var length = await connectionMultiplexer.GetDatabase().ListLengthAsync(key);

            // Assert
            length.ShouldBe(3);
        }

        [Fact]
        public async Task ListPushAsync_LeftDirection_PrependsValues()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "list:push:left:" + Guid.NewGuid();

            // Act
            await repo.ListPushAsync(groupId, key, ["a"], ListDirection.Right, null);
            await repo.ListPushAsync(groupId, key, ["z"], ListDirection.Left, null);

            var first = await connectionMultiplexer.GetDatabase().ListGetByIndexAsync(key, 0);

            // Assert
            first.ToString().ShouldBe("z");
        }

        [Fact]
        public async Task ListPushAsync_WithTtl_SetsExpiry()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "list:ttl:" + Guid.NewGuid();

            // Act
            await repo.ListPushAsync(groupId, key, ["a"], ListDirection.Right, TimeSpan.FromSeconds(60));

            var ttl = await connectionMultiplexer.GetDatabase().KeyTimeToLiveAsync(key);

            // Assert
            ttl.ShouldNotBeNull();
            ttl!.Value.TotalMilliseconds.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task ListRangeAsync_AllElements_ReturnsAllValues()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "list:range:" + Guid.NewGuid();

            // Act
            await connectionMultiplexer.GetDatabase().ListRightPushAsync(key, new RedisValue[] { "a", "b", "c" });

            var values = await repo.ListRangeAsync(groupId, key, 0, -1);

            // Assert
            values.ShouldNotBeEmpty();
            values.Count.ShouldBe(3);
            values.ShouldContain("a");
            values.ShouldContain("b");
            values.ShouldContain("c");
        }

        [Fact]
        public async Task ListRangeAsync_NonExistentKey_ReturnsEmpty()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);

            // Act
            var values = await repo.ListRangeAsync(groupId, "list:missing:" + Guid.NewGuid(), 0, -1);

            // Assert
            values.ShouldBeEmpty();
        }

        [Fact]
        public async Task ListRemoveAsync_ExistingValue_ReturnsRemovedCount()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "list:remove:" + Guid.NewGuid();

            // Act
            await connectionMultiplexer.GetDatabase().ListRightPushAsync(key, new RedisValue[] { "a", "b", "b", "c" });

            var removed = await repo.ListRemoveAsync(groupId, key, "b", 0);

            // Assert
            removed.ShouldBe(2L);
        }

        [Fact]
        public async Task ListRemoveAsync_NonExistentValue_ReturnsZero()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "list:remove:missing:" + Guid.NewGuid();

            // Act
            await connectionMultiplexer.GetDatabase().ListRightPushAsync(key, new RedisValue[] { "a", "b" });

            var removed = await repo.ListRemoveAsync(groupId, key, "z", 0);

            // Assert
            removed.ShouldBe(0L);
        }

        [Fact]
        public async Task ListPushAsync_EmptyGroupId_ThrowsArgumentException()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");

            // Act
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisListRepository repo = new RedisListRepository(connectionManager, CreateOptions());

            // Assert
            await Should.ThrowAsync<ArgumentException>(
                () => repo.ListPushAsync(Guid.Empty, "k", ["v"], ListDirection.Right, null));
        }

        [Fact]
        public async Task ListRangeAsync_EmptyGroupId_ThrowsArgumentException()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");

            // Act
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisListRepository repo = new RedisListRepository(connectionManager, CreateOptions());

            // Assert
            await Should.ThrowAsync<ArgumentException>(
                () => repo.ListRangeAsync(Guid.Empty, "k", 0, -1));
        }

        [Fact]
        public async Task ListRemoveAsync_EmptyGroupId_ThrowsArgumentException()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");

            // Act
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisListRepository repo = new RedisListRepository(connectionManager, CreateOptions());

            // Assert
            await Should.ThrowAsync<ArgumentException>(
                () => repo.ListRemoveAsync(Guid.Empty, "k", "v", 0));
        }

        private (IRedisListRepository repo, Guid groupId) CreateRepository(IConnectionMultiplexer connectionMultiplexer)
        {
            var options = CreateOptions();
            var connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisListRepository repo = new RedisListRepository(connectionManager, options);
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
                        Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                        Name = "list-test",
                        ConnectionString = _fixture.ConnectionString,
                        GroupType = GroupType.Standalone
                    }
                ]
            });
        }
    
}
}
