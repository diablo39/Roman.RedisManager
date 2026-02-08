using System.Text.Json.Serialization;

namespace Roman.RedisManager.Domain.Entities
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GroupType
    {
        Standalone,
        Cluster
    }
}
