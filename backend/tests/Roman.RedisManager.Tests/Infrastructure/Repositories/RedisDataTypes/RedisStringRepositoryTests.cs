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
    public class RedisStringRepositoryTests
    {
        private readonly RedisContainerFixture _fixture;

        public RedisStringRepositoryTests(RedisContainerFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task StringSetAsync_NewKey_ReturnsTrue()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "str:new:" + Guid.NewGuid();

            // Act
            var result = await repo.StringSetAsync(groupId, key, "value", null, SetCondition.None);

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public async Task StringSetAsync_WithTtl_SetsExpiry()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "str:ttl:" + Guid.NewGuid();

            // Act
            await repo.StringSetAsync(groupId, key, "value", TimeSpan.FromSeconds(60), SetCondition.None);

            var ttl = await connectionMultiplexer.GetDatabase().KeyTimeToLiveAsync(key);

            // Assert
            ttl.ShouldNotBeNull();
            ttl!.Value.TotalMilliseconds.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task StringSetAsync_NotExistsCondition_ExistingKey_ReturnsFalse()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "str:nx:" + Guid.NewGuid();

            // Act
            await connectionMultiplexer.GetDatabase().StringSetAsync(key, "original");

            var result = await repo.StringSetAsync(groupId, key, "new value", null, SetCondition.NotExists);

            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task StringSetAsync_ExistsCondition_NonExistentKey_ReturnsFalse()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "str:xx:nonexistent:" + Guid.NewGuid();

            // Act
            var result = await repo.StringSetAsync(groupId, key, "value", null, SetCondition.Exists);

            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task StringGetAsync_ExistingKey_ReturnsValue()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);
            var key = "str:get:" + Guid.NewGuid();

            // Act
            await connectionMultiplexer.GetDatabase().StringSetAsync(key, "expected value");

            var value = await repo.StringGetAsync(groupId, key);

            // Assert
            value.ShouldBe("expected value");
        }

        [Fact]
        public async Task StringGetAsync_NonExistentKey_ReturnsNull()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var (repo, groupId) = CreateRepository(connectionMultiplexer);

            // Act
            var value = await repo.StringGetAsync(groupId, "str:missing:" + Guid.NewGuid());

            // Assert
            value.ShouldBeNull();
        }

        [Fact]
        public async Task StringSetAsync_EmptyGroupId_ThrowsArgumentException()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");

            // Act
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisStringRepository repo = new RedisStringRepository(connectionManager, CreateOptions());

            // Assert
            await Should.ThrowAsync<ArgumentException>(
                () => repo.StringSetAsync(Guid.Empty, "k", "v", null, SetCondition.None));
        }

        [Fact]
        public async Task StringGetAsync_EmptyGroupId_ThrowsArgumentException()
        {

            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");

            // Act
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisStringRepository repo = new RedisStringRepository(connectionManager, CreateOptions());

            // Assert
            await Should.ThrowAsync<ArgumentException>(
                () => repo.StringGetAsync(Guid.Empty, "k"));
        }

        private (IRedisStringRepository repo, Guid groupId) CreateRepository(IConnectionMultiplexer connectionMultiplexer)
        {
            var options = CreateOptions();
            var connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisStringRepository repo = new RedisStringRepository(connectionManager, options);
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
                        Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                        Name = "string-test",
                        ConnectionString = _fixture.ConnectionString,
                        GroupType = GroupType.Standalone
                    }
                ]
            });
        }
    
}
}
