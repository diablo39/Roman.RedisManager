namespace Roman.RedisManager.Domain.Entities
{
    public class RedisServerGroup
    {
        public string Name { get; internal set; }

        public IEnumerable<string> Endpoints { get; internal set; }

        public GroupType GroupType { get; internal set; }

        public RedisServerGroup(string name, IEnumerable<string> endpoints, GroupType groupType)
        {
            Name = name;
            Endpoints = endpoints;
            GroupType = groupType;
        }

    }
}
