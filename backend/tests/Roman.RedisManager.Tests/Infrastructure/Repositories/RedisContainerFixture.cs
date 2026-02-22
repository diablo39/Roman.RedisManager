using Testcontainers.Redis;
using System.Threading.Tasks;

namespace Roman.RedisManager.Tests.Infrastructure.Repositories
{
    // shared collection fixture for Redis container
    public sealed class RedisContainerFixture : IAsyncLifetime
    {
        public RedisContainer Container { get; private set; } = null!;
        public string ConnectionString => Container.GetConnectionString();

        public async Task InitializeAsync()
        {
            Container = new RedisBuilder("redis:8.6.0").Build();
            await Container.StartAsync();
        }

        public async Task DisposeAsync()
        {
            if (Container is not null)
            {
                await Container.StopAsync();
            }
        }
    }

    [CollectionDefinition("Redis")]
    public class RedisCollection : ICollectionFixture<RedisContainerFixture>
    {
        // empty - just defines the shared context
    }
}