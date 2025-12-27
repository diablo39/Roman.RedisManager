using System;
using System.Collections.Generic;
using System.Text;

namespace Roman.RedisManager.Domain.Repositories
{
    public interface IRedisRepository
    {
        ReadisSearchResult SearchForKeys(string predicate);
    }

    public class RedisKey(string key)
    {
        public string Key { get; protected set; } = key;
    }

    public class ReadisSearchResult(IEnumerable<RedisKey> keys, long iterator)
    {
        public long Iterator { get; protected set; } = iterator;

        public IEnumerable<RedisKey> Keys { get; protected set; } = keys;
    }
}
