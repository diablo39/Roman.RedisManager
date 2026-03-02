using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Infrastructure.Exceptions;
using Roman.RedisManager.Infrastructure.Redis;
using StackExchange.Redis;
using SESortedSetEntry = StackExchange.Redis.SortedSetEntry;

namespace Roman.RedisManager.Infrastructure.Repositories
{
    public class RedisSortedSetRepository : IRedisSortedSetRepository
    {
        private readonly IRedisConnectionManager _connectionManager;
        private readonly IOptions<RedisConfiguration> _redisConfiguration;

        public RedisSortedSetRepository(IRedisConnectionManager connectionManager, IOptions<RedisConfiguration> redisConfiguration)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _redisConfiguration = redisConfiguration ?? throw new ArgumentNullException(nameof(redisConfiguration));
        }

        public async Task AddToSortedSetAsync(Guid groupId, string key, IReadOnlyCollection<RedisSortedSetEntry> entries, TimeSpan? ttl)
        {
            if (groupId == Guid.Empty)
            {
                throw new ArgumentException("Group identifier is required.", nameof(groupId));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key is required.", nameof(key));
            }

            ArgumentNullException.ThrowIfNull(entries);

            if (entries.Count == 0)
            {
                throw new ArgumentException("At least one entry must be provided.", nameof(entries));
            }

            _redisConfiguration.Value.ResolveServerGroup(groupId);

            try
            {
                var connection = await _connectionManager.GetConnectionAsync(groupId).ConfigureAwait(false);
                var db = connection.GetDatabase();

                var sortedSetEntries = entries.Select(e => new SESortedSetEntry(e.Member, e.Score)).ToArray();
                await db.SortedSetAddAsync(key, sortedSetEntries).ConfigureAwait(false);

                if (ttl.HasValue)
                {
                    await db.KeyExpireAsync(key, ttl.Value).ConfigureAwait(false);
                }
            }
            catch (RedisConnectionException ex)
            {
                throw new RedisConnectionFailureException("Failed to add entries to sorted set for the requested group.", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new RedisConnectionFailureException("Timed out while adding entries to sorted set.", ex);
            }
            catch (RedisServerException ex) when (ex.Message.Contains("WRONGTYPE", StringComparison.OrdinalIgnoreCase))
            {
                throw new RedisTypeMismatchException("The key holds a value of a different type.", ex);
            }
            catch (RedisServerException ex)
            {
                throw new RedisConnectionFailureException("Redis server returned an error while adding entries to sorted set.", ex);
            }
        }

        public async Task<IReadOnlyCollection<RedisSortedSetEntry>> GetSortedSetRangeAsync(Guid groupId, string key, long start, long stop)
        {
            if (groupId == Guid.Empty)
            {
                throw new ArgumentException("Group identifier is required.", nameof(groupId));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key is required.", nameof(key));
            }

            _redisConfiguration.Value.ResolveServerGroup(groupId);

            try
            {
                var connection = await _connectionManager.GetConnectionAsync(groupId).ConfigureAwait(false);
                var db = connection.GetDatabase();

                var seEntries = await db.SortedSetRangeByRankWithScoresAsync(key, start, stop).ConfigureAwait(false);

                return seEntries
                    .Select(e => new RedisSortedSetEntry(e.Element.ToString(), e.Score))
                    .ToList();
            }
            catch (RedisConnectionException ex)
            {
                throw new RedisConnectionFailureException("Failed to get sorted set range for the requested group.", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new RedisConnectionFailureException("Timed out while getting sorted set range.", ex);
            }
            catch (RedisServerException ex) when (ex.Message.Contains("WRONGTYPE", StringComparison.OrdinalIgnoreCase))
            {
                throw new RedisTypeMismatchException("The key holds a value of a different type.", ex);
            }
            catch (RedisServerException ex)
            {
                throw new RedisConnectionFailureException("Redis server returned an error while getting sorted set range.", ex);
            }
        }

        public async Task<long> RemoveFromSortedSetAsync(Guid groupId, string key, IReadOnlyCollection<string> members)
        {
            if (groupId == Guid.Empty)
            {
                throw new ArgumentException("Group identifier is required.", nameof(groupId));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key is required.", nameof(key));
            }

            ArgumentNullException.ThrowIfNull(members);

            if (members.Count == 0)
            {
                throw new ArgumentException("At least one member must be provided.", nameof(members));
            }

            _redisConfiguration.Value.ResolveServerGroup(groupId);

            try
            {
                var connection = await _connectionManager.GetConnectionAsync(groupId).ConfigureAwait(false);
                var db = connection.GetDatabase();

                var redisValues = members.Select(m => (RedisValue)m).ToArray();
                return await db.SortedSetRemoveAsync(key, redisValues).ConfigureAwait(false);
            }
            catch (RedisConnectionException ex)
            {
                throw new RedisConnectionFailureException("Failed to remove members from sorted set for the requested group.", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new RedisConnectionFailureException("Timed out while removing members from sorted set.", ex);
            }
            catch (RedisServerException ex) when (ex.Message.Contains("WRONGTYPE", StringComparison.OrdinalIgnoreCase))
            {
                throw new RedisTypeMismatchException("The key holds a value of a different type.", ex);
            }
            catch (RedisServerException ex)
            {
                throw new RedisConnectionFailureException("Redis server returned an error while removing from sorted set.", ex);
            }
        }
    }
}
