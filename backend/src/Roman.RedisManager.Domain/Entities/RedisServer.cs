namespace Roman.RedisManager.Domain.Entities
{
    public class RedisServer
    {
        public string Name { get; internal set; }

        public IEnumerable<string> Endpoints { get; internal set; }

        public RedisServer(string name, IEnumerable<string> endpoints)
        {
            Name = name;
            Endpoints = endpoints;
        }

    }
}
