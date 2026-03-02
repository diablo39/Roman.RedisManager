using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Infrastructure.Exceptions;
using Roman.RedisManager.Infrastructure.Redis;
using StackExchange.Redis;

namespace Roman.RedisManager.Infrastructure.Repositories
{
    public class RedisKeyRepository : IRedisKeyRepository
    {
        private readonly IRedisConnectionManager _connectionManager;
        private readonly IOptions<RedisConfiguration> _redisConfiguration;

        public RedisKeyRepository(IRedisConnectionManager connectionManager, IOptions<RedisConfiguration> redisConfiguration)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _redisConfiguration = redisConfiguration ?? throw new ArgumentNullException(nameof(redisConfiguration));
        }

        public async Task<bool> DeleteKeyAsync(Guid groupId, string key)
        {
            if (groupId == Guid.Empty)
            {
                throw new ArgumentException("Group identifier is required.", nameof(groupId));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key is required.", nameof(key));
            }
            
            // for validation purposes, we need to resolve the server group before attempting to get a connection. This ensures that if the groupId is invalid, we throw an exception before making any Redis calls.
            _redisConfiguration.Value.ResolveServerGroup(groupId);

            try
            {
                var connection = await _connectionManager.GetConnectionAsync(groupId).ConfigureAwait(false);
                var db = connection.GetDatabase();
                return await db.KeyDeleteAsync(key).ConfigureAwait(false);
            }
            catch (RedisConnectionException ex)
            {
                throw new RedisConnectionFailureException("Failed to delete key for the requested group.", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new RedisConnectionFailureException("Timed out while deleting key.", ex);
            }
            catch (RedisServerException ex)
            {
                throw new RedisConnectionFailureException("Redis server returned an error while deleting key.", ex);
            }
        }

        public async Task<RedisKeyMetadata> GetKeyMetadataAsync(Guid groupId, string key)
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

                var redisType = await db.KeyTypeAsync(key).ConfigureAwait(false);
                var ttl = await db.KeyTimeToLiveAsync(key).ConfigureAwait(false);

                var dataType = MapRedisType(redisType);
                return new RedisKeyMetadata(dataType, ttl);
            }
            catch (RedisConnectionException ex)
            {
                throw new RedisConnectionFailureException("Failed to retrieve key metadata for the requested group.", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new RedisConnectionFailureException("Timed out while retrieving key metadata.", ex);
            }
            catch (RedisServerException ex)
            {
                throw new RedisConnectionFailureException("Redis server returned an error while retrieving key metadata.", ex);
            }
        }

        public async Task<RedisKeyValue> GetKeyValueAsync(Guid groupId, string key)
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

                var redisType = await db.KeyTypeAsync(key).ConfigureAwait(false);
                var dataType = MapRedisType(redisType);

                switch (redisType)
                {
                    case RedisType.String:
                        var stringVal = await db.StringGetAsync(key).ConfigureAwait(false);
                        return new RedisKeyValue(dataType, stringValue: stringVal.IsNull ? null : (string?)stringVal);

                    case RedisType.List:
                        var listVals = await db.ListRangeAsync(key, 0, -1).ConfigureAwait(false);
                        return new RedisKeyValue(dataType, listValues: listVals.Select(v => (string?)v ?? string.Empty).ToList());

                    case RedisType.Set:
                        var setMembers = await db.SetMembersAsync(key).ConfigureAwait(false);
                        return new RedisKeyValue(dataType, setMembers: setMembers.Select(v => (string?)v ?? string.Empty).ToList());

                    case RedisType.Hash:
                        var hashEntries = await db.HashGetAllAsync(key).ConfigureAwait(false);
                        var hashFields = hashEntries.ToDictionary(
                            e => (string?)e.Name ?? string.Empty,
                            e => (string?)e.Value ?? string.Empty,
                            StringComparer.Ordinal);
                        return new RedisKeyValue(dataType, hashFields: hashFields);

                    case RedisType.SortedSet:
                        var sortedEntries = await db.SortedSetRangeByRankWithScoresAsync(key, 0, -1).ConfigureAwait(false);
                        var sortedSetEntries = sortedEntries
                            .Select(e => new RedisSortedSetEntry((string?)e.Element ?? string.Empty, e.Score))
                            .ToList();
                        return new RedisKeyValue(dataType, sortedSetEntries: sortedSetEntries);

                    default:
                        return new RedisKeyValue(dataType);
                }
            }
            catch (RedisConnectionException ex)
            {
                throw new RedisConnectionFailureException("Failed to retrieve key value for the requested group.", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new RedisConnectionFailureException("Timed out while retrieving key value.", ex);
            }
            catch (RedisServerException ex)
            {
                throw new RedisConnectionFailureException("Redis server returned an error while retrieving key value.", ex);
            }
        }

        private static RedisDataType MapRedisType(RedisType redisType) =>
            redisType switch
            {
                RedisType.String => RedisDataType.String,
                RedisType.List => RedisDataType.List,
                RedisType.Set => RedisDataType.Set,
                RedisType.Hash => RedisDataType.Hash,
                RedisType.SortedSet => RedisDataType.SortedSet,
                RedisType.Stream => RedisDataType.Stream,
                RedisType.None => RedisDataType.None,
                _ => RedisDataType.Unknown
            };
    }
}
