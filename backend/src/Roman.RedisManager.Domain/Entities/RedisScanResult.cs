namespace Roman.RedisManager.Domain.Entities
{
    public class RedisScanResult<T>
    {
        public long Cursor { get; }

        public IEnumerable<T> Items { get; }

        public bool HasMoreResults => Cursor != 0;

        public RedisScanResult(long cursor, IEnumerable<T> items)
        {
            Cursor = cursor;
            Items = items;
        }
    }
}
