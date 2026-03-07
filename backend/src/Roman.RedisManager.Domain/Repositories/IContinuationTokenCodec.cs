using Roman.RedisManager.Domain.Entities;

namespace Roman.RedisManager.Domain.Repositories
{
    public interface IContinuationTokenCodec
    {
        string Encode(ContinuationTokenEnvelope envelope);

        ContinuationTokenEnvelope Decode(string token);
    }
}
