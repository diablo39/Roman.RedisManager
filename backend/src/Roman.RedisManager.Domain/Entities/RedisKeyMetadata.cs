using Roman.RedisManager.Domain.Entities.RedisData;

namespace Roman.RedisManager.Domain.Entities
{
    public class RedisKeyMetadata
    {
        public RedisDataType Type { get; internal set; }

        public TimeSpan? Ttl { get; internal set; }

        public RedisKeyMetadata(RedisDataType type, TimeSpan? ttl)
        {
            Type = type;
            Ttl = ttl;
        }
    }
}
