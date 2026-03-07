using Roman.RedisManager.Domain.Entities.RedisData;

namespace Roman.RedisManager.Domain.Entities
{
    public class RedisKey(string key)
    {
        public RedisKey(string key, RedisDataType type, TimeSpan? ttl)
            : this(key, type, ttl, ttl.HasValue)
        {
        }

        public RedisKey(string key, RedisDataType type, TimeSpan? ttl, bool hasExpiration)
            : this(key)
        {
            Type = type;
            Ttl = ttl;
            HasExpiration = hasExpiration;
        }

        public string Key { get; protected set; } = key;

        public RedisDataType Type { get; protected set; } = RedisDataType.Unknown;

        public TimeSpan? Ttl { get; protected set; }

        public bool HasExpiration { get; protected set; }
    }
}
