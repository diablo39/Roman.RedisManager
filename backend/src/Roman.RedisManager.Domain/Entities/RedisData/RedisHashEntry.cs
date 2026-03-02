namespace Roman.RedisManager.Domain.Entities.RedisData
{
    public class RedisHashEntry
    {
        public string Field { get; }

        public string Value { get; }

        public RedisHashEntry(string field, string value)
        {
            Field = field;
            Value = value;
        }
    }
}
