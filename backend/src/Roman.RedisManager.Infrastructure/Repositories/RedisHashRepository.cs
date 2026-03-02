using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Infrastructure.Exceptions;
using Roman.RedisManager.Infrastructure.Redis;
using StackExchange.Redis;
using SEHashEntry = StackExchange.Redis.HashEntry;

namespace Roman.RedisManager.Infrastructure.Repositories
{
    public class RedisHashRepository : IRedisHashRepository
    {
        private readonly IRedisConnectionManager _connectionManager;
        private readonly IOptions<RedisConfiguration> _redisConfiguration;

        public RedisHashRepository(IRedisConnectionManager connectionManager, IOptions<RedisConfiguration> redisConfiguration)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _redisConfiguration = redisConfiguration ?? throw new ArgumentNullException(nameof(redisConfiguration));
        }

        public async Task SetHashFieldsAsync(Guid groupId, string key, IReadOnlyDictionary<string, string> fields, TimeSpan? ttl)
        {
            if (groupId == Guid.Empty)
            {
                throw new ArgumentException("Group identifier is required.", nameof(groupId));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key is required.", nameof(key));
            }

            ArgumentNullException.ThrowIfNull(fields);

            if (fields.Count == 0)
            {
                throw new ArgumentException("At least one field must be provided.", nameof(fields));
            }

            _redisConfiguration.Value.ResolveServerGroup(groupId);

            try
            {
                var connection = await _connectionManager.GetConnectionAsync(groupId).ConfigureAwait(false);
                var db = connection.GetDatabase();

                var hashEntries = fields.Select(kv => new SEHashEntry(kv.Key, kv.Value)).ToArray();
                await db.HashSetAsync(key, hashEntries).ConfigureAwait(false);

                if (ttl.HasValue)
                {
                    await db.KeyExpireAsync(key, ttl.Value).ConfigureAwait(false);
                }
            }
            catch (RedisConnectionException ex)
            {
                throw new RedisConnectionFailureException("Failed to set hash fields for the requested group.", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new RedisConnectionFailureException("Timed out while setting hash fields.", ex);
            }
            catch (RedisServerException ex) when (ex.Message.Contains("WRONGTYPE", StringComparison.OrdinalIgnoreCase))
            {
                throw new RedisTypeMismatchException("The key holds a value of a different type.", ex);
            }
            catch (RedisServerException ex)
            {
                throw new RedisConnectionFailureException("Redis server returned an error while setting hash fields.", ex);
            }
        }

        public async Task<RedisScanResult<RedisHashEntry>> HashScanAsync(Guid groupId, string key, long cursor, int pageSize)
        {
            if (groupId == Guid.Empty)
            {
                throw new ArgumentException("Group identifier is required.", nameof(groupId));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key is required.", nameof(key));
            }

            if (pageSize <= 0)
            {
                throw new ArgumentException("Page size must be a positive integer.", nameof(pageSize));
            }

            _redisConfiguration.Value.ResolveServerGroup(groupId);

            try
            {
                var connection = await _connectionManager.GetConnectionAsync(groupId).ConfigureAwait(false);
                var db = connection.GetDatabase();

                var scanResult = await db.ExecuteAsync(
                    "HSCAN",
                    key,
                    cursor.ToString(),
                    "COUNT",
                    pageSize.ToString()).ConfigureAwait(false);

                var inner = (RedisResult[])scanResult!;
                var nextCursor = long.Parse((string)inner[0]!);
                var rawPairs = (RedisResult[])inner[1]!;

                var entries = new List<RedisHashEntry>();
                for (var i = 0; i + 1 < rawPairs.Length; i += 2)
                {
                    var field = (string?)rawPairs[i] ?? string.Empty;
                    var value = (string?)rawPairs[i + 1] ?? string.Empty;
                    entries.Add(new RedisHashEntry(field, value));
                }

                return new RedisScanResult<RedisHashEntry>(nextCursor, entries);
            }
            catch (RedisConnectionException ex)
            {
                throw new RedisConnectionFailureException("Failed to scan hash fields for the requested group.", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new RedisConnectionFailureException("Timed out while scanning hash fields.", ex);
            }
            catch (RedisServerException ex) when (ex.Message.Contains("WRONGTYPE", StringComparison.OrdinalIgnoreCase))
            {
                throw new RedisTypeMismatchException("The key holds a value of a different type.", ex);
            }
            catch (RedisServerException ex)
            {
                throw new RedisConnectionFailureException("Redis server returned an error while scanning hash fields.", ex);
            }
        }

        public async Task<long> RemoveHashFieldsAsync(Guid groupId, string key, IReadOnlyCollection<string> fields)
        {
            if (groupId == Guid.Empty)
            {
                throw new ArgumentException("Group identifier is required.", nameof(groupId));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key is required.", nameof(key));
            }

            ArgumentNullException.ThrowIfNull(fields);

            if (fields.Count == 0)
            {
                throw new ArgumentException("At least one field must be provided.", nameof(fields));
            }

            _redisConfiguration.Value.ResolveServerGroup(groupId);

            try
            {
                var connection = await _connectionManager.GetConnectionAsync(groupId).ConfigureAwait(false);
                var db = connection.GetDatabase();

                var redisValues = fields.Select(f => (RedisValue)f).ToArray();
                return await db.HashDeleteAsync(key, redisValues).ConfigureAwait(false);
            }
            catch (RedisConnectionException ex)
            {
                throw new RedisConnectionFailureException("Failed to remove hash fields for the requested group.", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new RedisConnectionFailureException("Timed out while removing hash fields.", ex);
            }
            catch (RedisServerException ex) when (ex.Message.Contains("WRONGTYPE", StringComparison.OrdinalIgnoreCase))
            {
                throw new RedisTypeMismatchException("The key holds a value of a different type.", ex);
            }
            catch (RedisServerException ex)
            {
                throw new RedisConnectionFailureException("Redis server returned an error while removing hash fields.", ex);
            }
        }
    }
}
