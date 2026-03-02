using Roman.RedisManager.Domain.Entities;

namespace Roman.RedisManager.Domain.Repositories
{
    public interface IRedisHashRepository
    {
        Task SetHashFieldsAsync(Guid groupId, string key, IReadOnlyDictionary<string, string> fields, TimeSpan? ttl);

        Task<RedisScanResult<RedisHashEntry>> HashScanAsync(Guid groupId, string key, long cursor, int pageSize);

        Task<long> RemoveHashFieldsAsync(Guid groupId, string key, IReadOnlyCollection<string> fields);
    }
}
