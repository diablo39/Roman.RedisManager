using System.Text.Json.Serialization;

namespace Roman.RedisManager.Domain.Entities
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RedisDataType
    {
        None,
        String,
        List,
        Set,
        Hash,
        SortedSet,
        Stream,
        Unknown
    }
}
