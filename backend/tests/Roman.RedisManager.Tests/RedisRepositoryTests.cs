using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Extensions;
using Roman.RedisManager.Infrastructure.Configuration;
using Roman.RedisManager.Infrastructure.Redis;
using Roman.RedisManager.Infrastructure.Repositories;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace Roman.RedisManager.Tests
{
    public class RedisRepositoryTests
    {
        [Fact(Skip = "Requires local Redis instance")]
        public async Task SearchForKeysAsync_ShouldReturnNonNullAndNonEmptyKeys()
        {
            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect("localhost:6379");

            var options = Options.Create(new RedisConfiguration
            {
                ServerGroups =
                [
                    new RedisServerGroupConfiguration
                    {
                        Name = "placeholder",
                        ConnectionString = "localhost:6379",
                        GroupType = GroupType.Standalone
                    }
                ]
            });

            await using IRedisConnectionManager connectionManager = new SimpleConnectionManager(connectionMultiplexer);

            IRedisRepository redisRepository = new RedisRepository(connectionManager, options);
            var groupId = "placeholder".ToMd5Hash();

            // Act
            RedisSearchResult result = await redisRepository.SearchForKeysAsync(groupId, predicate: string.Empty);

            // Assert
            result.ShouldNotBeNull();
            result.Keys.ShouldNotBeEmpty();
        }

        private sealed class SimpleConnectionManager : IRedisConnectionManager
        {
            private readonly IConnectionMultiplexer _multiplexer;

            public SimpleConnectionManager(IConnectionMultiplexer multiplexer) => _multiplexer = multiplexer;

            public Task<IConnectionMultiplexer> GetConnectionAsync(string groupId) => Task.FromResult(_multiplexer);

            public ValueTask DisposeAsync()
            {
                _multiplexer.Dispose();
                return ValueTask.CompletedTask;
            }
        }

    }
}
