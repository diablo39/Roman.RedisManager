namespace Roman.RedisManager.Domain.Entities
{
    public record ClusterCursorState(int NodeIndex, long NodeCursor, string? TopologyFingerprint);
}
