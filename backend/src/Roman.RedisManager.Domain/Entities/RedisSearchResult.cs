namespace Roman.RedisManager.Domain.Entities
{
    public class RedisSearchResult(IEnumerable<RedisKey> keys, bool hasMoreResults)
    {
        private const long _emptyCursor = 0;

        public RedisSearchResult(IEnumerable<RedisKey> keys, long cursor)
            : this(keys, cursor != _emptyCursor)
        {
            Cursor = cursor;
        }

        public long Cursor { get; protected set; }

        public string? ContinuationToken { get; set; }

        public IEnumerable<RedisKey> Keys { get; protected set; } = keys;

        public Dictionary<string, long>? NodeCursors { get; init; }

        public bool HasMoreResults { get; protected set; } = hasMoreResults;
    }
}
