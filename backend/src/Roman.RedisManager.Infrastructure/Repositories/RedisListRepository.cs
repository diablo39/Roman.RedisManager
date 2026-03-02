using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Infrastructure.Exceptions;
using Roman.RedisManager.Infrastructure.Redis;
using StackExchange.Redis;

namespace Roman.RedisManager.Infrastructure.Repositories
{
    public class RedisListRepository : IRedisListRepository
    {
        private readonly IRedisConnectionManager _connectionManager;
        private readonly IOptions<RedisConfiguration> _redisConfiguration;

        public RedisListRepository(IRedisConnectionManager connectionManager, IOptions<RedisConfiguration> redisConfiguration)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _redisConfiguration = redisConfiguration ?? throw new ArgumentNullException(nameof(redisConfiguration));
        }

        public async Task ListPushAsync(Guid groupId, string key, IReadOnlyCollection<string> values, ListDirection direction, TimeSpan? ttl)
        {
            if (groupId == Guid.Empty)
            {
                throw new ArgumentException("Group identifier is required.", nameof(groupId));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key is required.", nameof(key));
            }

            ArgumentNullException.ThrowIfNull(values);

            if (values.Count == 0)
            {
                throw new ArgumentException("At least one value must be provided.", nameof(values));
            }

            _redisConfiguration.Value.ResolveServerGroup(groupId);

            try
            {
                var connection = await _connectionManager.GetConnectionAsync(groupId).ConfigureAwait(false);
                var db = connection.GetDatabase();

                var redisValues = values.Select(v => (RedisValue)v).ToArray();

                if (direction == ListDirection.Left)
                {
                    await db.ListLeftPushAsync(key, redisValues).ConfigureAwait(false);
                }
                else
                {
                    await db.ListRightPushAsync(key, redisValues).ConfigureAwait(false);
                }

                if (ttl.HasValue)
                {
                    await db.KeyExpireAsync(key, ttl.Value).ConfigureAwait(false);
                }
            }
            catch (RedisConnectionException ex)
            {
                throw new RedisConnectionFailureException("Failed to push values to list for the requested group.", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new RedisConnectionFailureException("Timed out while pushing values to list.", ex);
            }
            catch (RedisServerException ex) when (ex.Message.Contains("WRONGTYPE", StringComparison.OrdinalIgnoreCase))
            {
                throw new RedisTypeMismatchException("The key holds a value of a different type.", ex);
            }
            catch (RedisServerException ex)
            {
                throw new RedisConnectionFailureException("Redis server returned an error while pushing values to list.", ex);
            }
        }

        public async Task<IReadOnlyCollection<string>> ListRangeAsync(Guid groupId, string key, long start, long stop)
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

                var values = await db.ListRangeAsync(key, start, stop).ConfigureAwait(false);
                return values.Select(v => (string?)v ?? string.Empty).ToList();
            }
            catch (RedisConnectionException ex)
            {
                throw new RedisConnectionFailureException("Failed to retrieve list range for the requested group.", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new RedisConnectionFailureException("Timed out while retrieving list range.", ex);
            }
            catch (RedisServerException ex) when (ex.Message.Contains("WRONGTYPE", StringComparison.OrdinalIgnoreCase))
            {
                throw new RedisTypeMismatchException("The key holds a value of a different type.", ex);
            }
            catch (RedisServerException ex)
            {
                throw new RedisConnectionFailureException("Redis server returned an error while retrieving list range.", ex);
            }
        }

        public async Task<long> ListRemoveAsync(Guid groupId, string key, string value, long count)
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
                return await db.ListRemoveAsync(key, value, count).ConfigureAwait(false);
            }
            catch (RedisConnectionException ex)
            {
                throw new RedisConnectionFailureException("Failed to remove elements from list for the requested group.", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new RedisConnectionFailureException("Timed out while removing elements from list.", ex);
            }
            catch (RedisServerException ex) when (ex.Message.Contains("WRONGTYPE", StringComparison.OrdinalIgnoreCase))
            {
                throw new RedisTypeMismatchException("The key holds a value of a different type.", ex);
            }
            catch (RedisServerException ex)
            {
                throw new RedisConnectionFailureException("Redis server returned an error while removing elements from list.", ex);
            }
        }
    }
}
