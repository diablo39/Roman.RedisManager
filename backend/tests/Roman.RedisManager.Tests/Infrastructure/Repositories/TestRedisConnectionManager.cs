using Roman.RedisManager.Infrastructure.Redis;
using StackExchange.Redis;

namespace Roman.RedisManager.Tests.Infrastructure.Repositories
{
    internal sealed class TestRedisConnectionManager : IRedisConnectionManager
    {
        private readonly IConnectionMultiplexer _multiplexer;

        public TestRedisConnectionManager(IConnectionMultiplexer multiplexer) => _multiplexer = multiplexer;

        public Task<IConnectionMultiplexer> GetConnectionAsync(Guid groupId) => Task.FromResult(_multiplexer);

        public ValueTask DisposeAsync()
        {
            _multiplexer.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
