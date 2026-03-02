namespace Roman.RedisManager.Domain.Entities.Server
{
    public class RedisServerGroup
    {
        public Guid Id { get; internal set; }

        public string Name { get; internal set; }

        public string ConnectionString { get; internal set; }

        public GroupType GroupType { get; internal set; }

        public RedisServerGroup(Guid id, string name, string connectionString, GroupType groupType)
        {
            Id = id;
            Name = name;
            ConnectionString = connectionString;
            GroupType = groupType;
        }

    }
}
