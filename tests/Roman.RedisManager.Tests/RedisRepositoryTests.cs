using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Infrastructure.Repositories;

namespace Roman.RedisManager.Tests
{
    public class RedisRepositoryTests
    {
        [Fact]
        public void SearchForKeys_ShouldReturnNonNullAndNonEmptyKeys()
        {
            // Arrange
            IRedisRepository redisRepository = new RedisRepository();

            // Act
            ReadisSearchResult result = redisRepository.SearchForKeys(predicate: string.Empty);

            //// Assert
            result.ShouldNotBeNull();
            result.Keys.ShouldNotBeEmpty();
            result.Iterator.ShouldNotBeNullOrWhiteSpace();
        }
    }
}
