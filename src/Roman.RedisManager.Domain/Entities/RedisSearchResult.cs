namespace Roman.RedisManager.Domain.Entities
{
    public class RedisSearchResult(IEnumerable<RedisKey> keys, long cursor)
    {
        private const long _emptyCursor = 0;

        public long Cursor { get; protected set; } = cursor;

        public IEnumerable<RedisKey> Keys { get; protected set; } = keys;

        public bool HasMoreResults
        {
            get
            {
                return Cursor != _emptyCursor;
            }
        }
    }
}
