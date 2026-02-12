namespace Roman.RedisManager.Domain.Entities
{
    public class RedisServerGroup
    {
        public string Name { get; internal set; }

        public string ConnectionString { get; internal set; }

        public GroupType GroupType { get; internal set; }

        public RedisServerGroup(string name, string connectionString, GroupType groupType)
        {
            Name = name;
            ConnectionString = connectionString;
            GroupType = groupType;
        }

    }
}
