using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Domain.Entities.Server;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Domain.Repositories.RedisDataTypes;
using Roman.RedisManager.Infrastructure.Redis;
using Roman.RedisManager.Infrastructure.Repositories.RedisDataTypes;
using StackExchange.Redis;

namespace Roman.RedisManager.Tests.Infrastructure.Repositories.RedisDataTypes
{
    [Collection("Redis")]
    public class RedisHashRepositoryTests
    {
        private readonly RedisContainerFixture _fixture;

        public RedisHashRepositoryTests(RedisContainerFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task SetHashFieldsAsync_NewFields_SetsFields()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "hash:set:" + Guid.NewGuid();

            // Act
            await repo.SetHashFieldsAsync(groupId, key, new Dictionary<string, string> { ["name"] = "Alice", ["age"] = "30" }, null);

            var count = await connectionMultiplexer.GetDatabase().HashLengthAsync(key);

            // Assert
            count.ShouldBe(2);
        }

        [Fact]
        public async Task SetHashFieldsAsync_WithTtl_SetsExpiry()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "hash:ttl:" + Guid.NewGuid();

            // Act
            await repo.SetHashFieldsAsync(groupId, key, new Dictionary<string, string> { ["f"] = "v" }, TimeSpan.FromSeconds(60));

            var ttl = await connectionMultiplexer.GetDatabase().KeyTimeToLiveAsync(key);

            // Assert
            ttl.ShouldNotBeNull();
            ttl!.Value.TotalMilliseconds.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task HashScanAsync_ExistingHash_ReturnsFields()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "hash:scan:" + Guid.NewGuid();

            // Act
            await connectionMultiplexer.GetDatabase().HashSetAsync(key, new HashEntry[]
            {
                new("name", "Bob"),
                new("city", "Warsaw")
            });

            var result = await repo.HashScanAsync(groupId, key, 0, 100);

            // Assert
            result.ShouldNotBeNull();
            result.Items.ShouldNotBeEmpty();
            result.Items.ShouldContain(e => e.Field == "name" && e.Value == "Bob");
            result.Items.ShouldContain(e => e.Field == "city" && e.Value == "Warsaw");
            result.HasMoreResults.ShouldBeFalse();
        }

        [Fact]
        public async Task HashScanAsync_NonExistentKey_ReturnsEmpty()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);

            // Act
            var result = await repo.HashScanAsync(groupId, "hash:missing:" + Guid.NewGuid(), 0, 100);

            // Assert
            result.ShouldNotBeNull();
            result.Items.ShouldBeEmpty();
            result.HasMoreResults.ShouldBeFalse();
        }

        [Fact]
        public async Task RemoveHashFieldsAsync_ExistingFields_ReturnsRemovedCount()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "hash:remove:" + Guid.NewGuid();

            // Act
            await connectionMultiplexer.GetDatabase().HashSetAsync(key, new HashEntry[]
            {
                new("name", "Alice"),
                new("age", "30"),
                new("city", "London")
            });

            var removed = await repo.RemoveHashFieldsAsync(groupId, key, ["name", "age"]);

            // Assert
            removed.ShouldBe(2L);
        }

        [Fact]
        public async Task RemoveHashFieldsAsync_NonExistentField_ReturnsZero()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "hash:remove:missing:" + Guid.NewGuid();

            // Act
            await connectionMultiplexer.GetDatabase().HashSetAsync(key, new HashEntry[] { new("f", "v") });

            var removed = await repo.RemoveHashFieldsAsync(groupId, key, ["nonexistent"]);

            // Assert
            removed.ShouldBe(0L);
        }

        [Fact]
        public async Task SetHashFieldsAsync_EmptyGroupId_ThrowsArgumentException()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");

            // Act
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisHashRepository repo = new RedisHashRepository(connectionManager, CreateOptions());

            // Assert
            await Should.ThrowAsync<ArgumentException>(
                () => repo.SetHashFieldsAsync(Guid.Empty, "k", new Dictionary<string, string> { ["f"] = "v" }, null));
        }

        [Fact]
        public async Task HashScanAsync_EmptyGroupId_ThrowsArgumentException()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");

            // Act
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisHashRepository repo = new RedisHashRepository(connectionManager, CreateOptions());

            // Assert
            await Should.ThrowAsync<ArgumentException>(
                () => repo.HashScanAsync(Guid.Empty, "k", 0, 100));
        }

        [Fact]
        public async Task RemoveHashFieldsAsync_EmptyGroupId_ThrowsArgumentException()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");

            // Act
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisHashRepository repo = new RedisHashRepository(connectionManager, CreateOptions());

            // Assert
            await Should.ThrowAsync<ArgumentException>(
                () => repo.RemoveHashFieldsAsync(Guid.Empty, "k", ["f"]));
        }

        private (IRedisHashRepository repo, Guid groupId) CreateRepository(IConnectionMultiplexer connectionMultiplexer)
        {
            var options = CreateOptions();
            var connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
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
    
}
}
