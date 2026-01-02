using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Infrastructure.Repositories;
using StackExchange.Redis;

namespace Roman.RedisManager.Tests
{
    public class RedisRepositoryTests
    {
        [Fact]
        public void SearchForKeys_ShouldReturnNonNullAndNonEmptyKeys()
        {
            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect("localhost:6379");
            IRedisRepository redisRepository = new RedisRepository(connectionMultiplexer);

            // Act
            RedisSearchResult result = redisRepository.SearchForKeys(predicate: string.Empty);

            //// Assert
            result.ShouldNotBeNull();
            result.Keys.ShouldNotBeEmpty();
            
        }

    }
}
