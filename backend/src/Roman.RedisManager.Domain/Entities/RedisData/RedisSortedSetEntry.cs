namespace Roman.RedisManager.Domain.Entities.RedisData
{
    public class RedisSortedSetEntry
    {
        public string Member { get; internal set; }

        public double Score { get; internal set; }

        public RedisSortedSetEntry(string member, double score)
        {
            Member = member;
            Score = score;
        }
    }
}
