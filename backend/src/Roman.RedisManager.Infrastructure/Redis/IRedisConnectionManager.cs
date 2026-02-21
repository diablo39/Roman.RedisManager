using StackExchange.Redis;
using System.Threading.Tasks;

namespace Roman.RedisManager.Infrastructure.Redis
{
    public interface IRedisConnectionManager : IAsyncDisposable
    {
        Task<IConnectionMultiplexer> GetConnectionAsync(Guid groupId);
    }
}
