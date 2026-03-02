using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Infrastructure.Exceptions;
using Roman.RedisManager.Infrastructure.Redis;
using StackExchange.Redis;

namespace Roman.RedisManager.Infrastructure.Repositories
{
    public class RedisStringRepository : IRedisStringRepository
    {
        private readonly IRedisConnectionManager _connectionManager;
        private readonly IOptions<RedisConfiguration> _redisConfiguration;

        public RedisStringRepository(IRedisConnectionManager connectionManager, IOptions<RedisConfiguration> redisConfiguration)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _redisConfiguration = redisConfiguration ?? throw new ArgumentNullException(nameof(redisConfiguration));
        }

        public async Task<bool> StringSetAsync(Guid groupId, string key, string value, TimeSpan? ttl, SetCondition condition)
        {
            if (groupId == Guid.Empty)
            {
                throw new ArgumentException("Group identifier is required.", nameof(groupId));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key is required.", nameof(key));
            }

            ArgumentNullException.ThrowIfNull(value);

            _redisConfiguration.Value.ResolveServerGroup(groupId);

            var when = condition switch
            {
                SetCondition.NotExists => When.NotExists,
                SetCondition.Exists => When.Exists,
                _ => When.Always
            };

            try
            {
                var connection = await _connectionManager.GetConnectionAsync(groupId).ConfigureAwait(false);
                var db = connection.GetDatabase();
                return await db.StringSetAsync(key, value, ttl, when).ConfigureAwait(false);
            }
            catch (RedisConnectionException ex)
            {
                throw new RedisConnectionFailureException("Failed to set string value for the requested group.", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new RedisConnectionFailureException("Timed out while setting string value.", ex);
            }
            catch (RedisServerException ex) when (ex.Message.Contains("WRONGTYPE", StringComparison.OrdinalIgnoreCase))
            {
                throw new RedisTypeMismatchException("The key holds a value of a different type.", ex);
            }
            catch (RedisServerException ex)
            {
                throw new RedisConnectionFailureException("Redis server returned an error while setting string value.", ex);
            }
        }

        public async Task<string?> StringGetAsync(Guid groupId, string key)
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
                var result = await db.StringGetAsync(key).ConfigureAwait(false);
                return result.IsNull ? null : (string?)result;
            }
            catch (RedisConnectionException ex)
            {
                throw new RedisConnectionFailureException("Failed to get string value for the requested group.", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new RedisConnectionFailureException("Timed out while getting string value.", ex);
            }
            catch (RedisServerException ex) when (ex.Message.Contains("WRONGTYPE", StringComparison.OrdinalIgnoreCase))
            {
                throw new RedisTypeMismatchException("The key holds a value of a different type.", ex);
            }
            catch (RedisServerException ex)
            {
                throw new RedisConnectionFailureException("Redis server returned an error while getting string value.", ex);
            }
        }
    }
}
