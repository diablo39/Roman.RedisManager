using Roman.RedisManager.Domain.Entities.RedisData;

namespace Roman.RedisManager.Domain.Repositories.RedisDataTypes
{
    public interface IRedisStringRepository
    {
        Task<bool> StringSetAsync(Guid groupId, string key, string value, TimeSpan? ttl, SetCondition condition);

        Task<string?> StringGetAsync(Guid groupId, string key);
    }
}
