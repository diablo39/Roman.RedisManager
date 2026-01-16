namespace Roman.RedisManager.Domain.Entities
{
    public class RedisKey(string key)
    {
        public string Key { get; protected set; } = key;
    }
}
