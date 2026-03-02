using Roman.RedisManager.Domain.Entities;

namespace Roman.RedisManager.Domain.Repositories
{
    public interface IRedisSetRepository
    {
        Task SetAddAsync(Guid groupId, string key, IReadOnlyCollection<string> members, TimeSpan? ttl);

        Task<RedisScanResult<string>> SetScanAsync(Guid groupId, string key, long cursor, int pageSize);

        Task<long> SetRemoveAsync(Guid groupId, string key, IReadOnlyCollection<string> members);
    }
}
