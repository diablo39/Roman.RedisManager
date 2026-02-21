using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Extensions;
using Roman.RedisManager.Infrastructure.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Roman.RedisManager.Infrastructure.Redis
{
    /// <summary>
    /// Provides access to Redis connections scoped by a server group identifier, caching connections for reuse.
    /// </summary>
    /// <remarks>
    /// Uses a thread-safe cache to create a single connection per group and removes failed entries on error.
    /// </remarks>
    public sealed class RedisConnectionManager : IRedisConnectionManager
    {
        private readonly IOptions<RedisConfiguration> _redisConfiguration;
        private readonly ConcurrentDictionary<string, Task<IConnectionMultiplexer>> _connections = new();

        public RedisConnectionManager(IOptions<RedisConfiguration> redisConfiguration)
        {
            _redisConfiguration = redisConfiguration ?? throw new ArgumentNullException(nameof(redisConfiguration));
        }

        public async Task<IConnectionMultiplexer> GetConnectionAsync(Guid groupId)
        {
            if (groupId == Guid.Empty)
            {
                throw new ArgumentException("Group identifier cannot be empty.", nameof(groupId));
            }

            var groupKey = groupId.ToString();
            var serverGroup = _redisConfiguration.Value.ResolveServerGroup(groupId);
            var connectionTask = _connections.GetOrAdd(groupKey, _ => CreateConnectionAsync(serverGroup));

            try
            {
                return await connectionTask.ConfigureAwait(false);
            }
            catch
            {
                _connections.TryRemove(groupKey, out _);
                throw;
            }
        }

        private static async Task<IConnectionMultiplexer> CreateConnectionAsync(RedisServerGroupConfiguration serverGroup)
        {
            ArgumentNullException.ThrowIfNull(serverGroup, nameof(serverGroup));

            var multiplexer = await ConnectionMultiplexer
                .ConnectAsync(serverGroup.ConnectionString)
                .ConfigureAwait(false);

            return multiplexer;
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var connectionTask in _connections.Values)
            {
                var connection = await connectionTask.ConfigureAwait(false);
                connection.Dispose();
            }

            _connections.Clear();
        }
    }
}
