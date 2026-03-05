using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories.RedisDataTypes;
using Roman.RedisManager.Infrastructure.Exceptions;
using Roman.RedisManager.Infrastructure.Redis;
using StackExchange.Redis;

namespace Roman.RedisManager.Infrastructure.Repositories.RedisDataTypes
{
    public class RedisSetRepository : IRedisSetRepository
    {
        private readonly IRedisConnectionManager _connectionManager;
        private readonly IOptions<RedisConfiguration> _redisConfiguration;

        public RedisSetRepository(IRedisConnectionManager connectionManager, IOptions<RedisConfiguration> redisConfiguration)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _redisConfiguration = redisConfiguration ?? throw new ArgumentNullException(nameof(redisConfiguration));
        }

        public async Task SetAddAsync(Guid groupId, string key, IReadOnlyCollection<string> members, TimeSpan? ttl)
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
                await db.SetAddAsync(key, redisValues).ConfigureAwait(false);

                if (ttl.HasValue)
                {
                    await db.KeyExpireAsync(key, ttl.Value).ConfigureAwait(false);
                }
            }
            catch (RedisConnectionException ex)
            {
                throw new RedisConnectionFailureException("Failed to add members to set for the requested group.", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new RedisConnectionFailureException("Timed out while adding members to set.", ex);
            }
            catch (RedisServerException ex) when (ex.Message.Contains("WRONGTYPE", StringComparison.OrdinalIgnoreCase))
            {
                throw new RedisTypeMismatchException("The key holds a value of a different type.", ex);
            }
            catch (RedisServerException ex)
            {
                throw new RedisConnectionFailureException("Redis server returned an error while adding members to set.", ex);
            }
        }

        public async Task<RedisScanResult<string>> SetScanAsync(Guid groupId, string key, long cursor, int pageSize)
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
                    "SSCAN",
                    key,
                    cursor.ToString(),
                    "COUNT",
                    pageSize.ToString()).ConfigureAwait(false);

                var inner = (RedisResult[])scanResult!;
                var nextCursor = long.Parse((string)inner[0]!);
                var rawMembers = (RedisResult[])inner[1]!;
                var members = rawMembers.Select(m => (string?)m ?? string.Empty).ToList();

                return new RedisScanResult<string>(nextCursor, members);
            }
            catch (RedisConnectionException ex)
            {
                throw new RedisConnectionFailureException("Failed to scan set members for the requested group.", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new RedisConnectionFailureException("Timed out while scanning set members.", ex);
            }
            catch (RedisServerException ex) when (ex.Message.Contains("WRONGTYPE", StringComparison.OrdinalIgnoreCase))
            {
                throw new RedisTypeMismatchException("The key holds a value of a different type.", ex);
            }
            catch (RedisServerException ex)
            {
                throw new RedisConnectionFailureException("Redis server returned an error while scanning set members.", ex);
            }
        }

        public async Task<long> SetRemoveAsync(Guid groupId, string key, IReadOnlyCollection<string> members)
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
                return await db.SetRemoveAsync(key, redisValues).ConfigureAwait(false);
            }
            catch (RedisConnectionException ex)
            {
                throw new RedisConnectionFailureException("Failed to remove members from set for the requested group.", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new RedisConnectionFailureException("Timed out while removing members from set.", ex);
            }
            catch (RedisServerException ex) when (ex.Message.Contains("WRONGTYPE", StringComparison.OrdinalIgnoreCase))
            {
                throw new RedisTypeMismatchException("The key holds a value of a different type.", ex);
            }
            catch (RedisServerException ex)
            {
                throw new RedisConnectionFailureException("Redis server returned an error while removing members from set.", ex);
            }
        }
    }
}
