namespace Roman.RedisManager.Domain.Entities
{
    public enum ContinuationTokenMode
    {
        Standalone,
        Cluster
    }

    public class ContinuationTokenEnvelope
    {
        public byte Version { get; set; } = 1;

        public required string ContextHash { get; set; }

        public required ContinuationTokenMode Mode { get; set; }

        public DateTimeOffset IssuedAtUtc { get; set; } = DateTimeOffset.UtcNow;

        public StandaloneCursorState? StandaloneState { get; set; }

        public ClusterCursorState? ClusterState { get; set; }
    }
}
