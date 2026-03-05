using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Entities.RedisData;

namespace Roman.RedisManager.Domain.Repositories.RedisDataTypes
{
    public interface IRedisHashRepository
    {
        Task SetHashFieldsAsync(Guid groupId, string key, IReadOnlyDictionary<string, string> fields, TimeSpan? ttl);

        Task<RedisScanResult<RedisHashEntry>> HashScanAsync(Guid groupId, string key, long cursor, int pageSize);

        Task<long> RemoveHashFieldsAsync(Guid groupId, string key, IReadOnlyCollection<string> fields);
    }
}
