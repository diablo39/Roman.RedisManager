using Roman.RedisManager.Domain.Entities.RedisData;

namespace Roman.RedisManager.Domain.Entities
{
    public class RedisKeyValue(
        RedisDataType type,
        string? stringValue = null,
        IEnumerable<string>? listValues = null,
        IEnumerable<string>? setMembers = null,
        IReadOnlyDictionary<string, string>? hashFields = null,
        IEnumerable<RedisSortedSetEntry>? sortedSetEntries = null)
    {
        public RedisDataType Type { get; } = type;

        public string? StringValue { get; } = stringValue;

        public IEnumerable<string>? ListValues { get; } = listValues;

        public IEnumerable<string>? SetMembers { get; } = setMembers;

        public IReadOnlyDictionary<string, string>? HashFields { get; } = hashFields;

        public IEnumerable<RedisSortedSetEntry>? SortedSetEntries { get; } = sortedSetEntries;
    }
}
