namespace Roman.RedisManager.Domain.Entities
{
    public class ReadisSearchResult(IEnumerable<RedisKey> keys, long iterator)
    {
        public long Iterator { get; protected set; } = iterator;

        public IEnumerable<RedisKey> Keys { get; protected set; } = keys;
    }
}
