using Roman.RedisManager.Domain.Entities;

namespace Roman.RedisManager.Domain.Repositories
{
    public interface IRedisStringRepository
    {
        Task<bool> StringSetAsync(Guid groupId, string key, string value, TimeSpan? ttl, SetCondition condition);

        Task<string?> StringGetAsync(Guid groupId, string key);
    }
}
