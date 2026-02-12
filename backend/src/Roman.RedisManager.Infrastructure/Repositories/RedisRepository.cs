using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Extensions;
using Roman.RedisManager.Infrastructure.Configuration;
using Roman.RedisManager.Infrastructure.Exceptions;
using Roman.RedisManager.Infrastructure.Redis;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RedisKey = Roman.RedisManager.Domain.Entities.RedisKey;

namespace Roman.RedisManager.Infrastructure.Repositories
{
    public class RedisRepository : IRedisRepository
    {
        private readonly IRedisConnectionManager _connectionManager;
        private readonly IOptions<RedisConfiguration> _redisConfiguration;

        public RedisRepository(IRedisConnectionManager connectionManager, IOptions<RedisConfiguration> redisConfiguration)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _redisConfiguration = redisConfiguration ?? throw new ArgumentNullException(nameof(redisConfiguration));
        }

        public async Task<RedisSearchResult> SearchForKeysAsync(string groupId, string predicate)
        {
            if (string.IsNullOrWhiteSpace(groupId))
            {
                throw new ArgumentException("Group identifier is required.", nameof(groupId));
            }

            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            try
            {
                var connection = await _connectionManager.GetConnectionAsync(groupId).ConfigureAwait(false);
                var server = GetServer(connection);

                var searchResult = server.Keys(pattern: predicate);
                var cursor = (IScanningCursor)searchResult;
                var keys = searchResult.Select(e => new RedisKey(e.ToString())).ToList();

                return new RedisSearchResult(keys, cursor.Cursor);
            }
            catch (RedisConnectionException ex)
            {
                throw new RedisConnectionFailureException("Failed to search keys for the requested group.", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new RedisConnectionFailureException("Timed out while searching keys for the requested group.", ex);
            }
            catch (RedisServerException ex)
            {
                throw new RedisConnectionFailureException("Redis server returned an error while searching keys for the requested group.", ex);
            }
        }

        public async Task<IReadOnlyCollection<RedisServerNode>> GetServerNodesAsync(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId))
            {
                throw new ArgumentException("Group identifier is required.", nameof(groupId));
            }

            var serverGroup = _redisConfiguration.Value.ResolveServerGroup(groupId);

            try
            {
                var connection = await _connectionManager.GetConnectionAsync(groupId).ConfigureAwait(false);
                var server = GetServer(connection);

                var nodes = serverGroup.GroupType == GroupType.Cluster
                    ? await ReadClusterNodesAsync(server).ConfigureAwait(false)
                    : await ReadStandaloneNodesAsync(server).ConfigureAwait(false);

                return nodes
                    .GroupBy(node => new { node.Host, node.Port, node.Role })
                    .Select(group => group.First())
                    .ToList();
            }
            catch (RedisConnectionException ex)
            {
                throw new RedisConnectionFailureException("Failed to connect to the Redis topology for the requested group.", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new RedisConnectionFailureException("Timed out while retrieving Redis server nodes.", ex);
            }
            catch (RedisServerException ex)
            {
                throw new RedisConnectionFailureException("Redis server returned an error while retrieving server nodes.", ex);
            }
        }

        

        private static IServer GetServer(IConnectionMultiplexer connection)
        {
            var endpoint = connection.GetEndPoints().FirstOrDefault()
                ?? throw new InvalidOperationException("Redis connection does not expose endpoints.");

            return connection.GetServer(endpoint);
        }

        private static async Task<List<RedisServerNode>> ReadStandaloneNodesAsync(IServer server)
        {
            var nodes = new List<RedisServerNode>();
            var (host, port) = ResolveEndpoint(server.EndPoint);
            nodes.Add(new RedisServerNode(host, port, "master"));

            var sections = await server.InfoAsync("replication").ConfigureAwait(false);
            var replicationSection = sections.FirstOrDefault();

            if (replicationSection is null)
            {
                return nodes;
            }

            foreach (var entry in replicationSection.Where(kvp => kvp.Key.StartsWith("slave", StringComparison.OrdinalIgnoreCase)))
            {
                var attributes = entry.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);

                var ipValue = attributes.FirstOrDefault(value => value.StartsWith("ip=", StringComparison.OrdinalIgnoreCase))?.Split('=')[1];
                var portValue = attributes.FirstOrDefault(value => value.StartsWith("port=", StringComparison.OrdinalIgnoreCase))?.Split('=')[1];

                if (string.IsNullOrWhiteSpace(ipValue) ||
                    string.IsNullOrWhiteSpace(portValue) ||
                    !int.TryParse(portValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPort))
                {
                    continue;
                }

                nodes.Add(new RedisServerNode(ipValue, parsedPort, "slave"));
            }

            return nodes;
        }

        private static async Task<List<RedisServerNode>> ReadClusterNodesAsync(IServer server)
        {
            var clusterNodes = await server.ClusterNodesAsync().ConfigureAwait(false);

            if (clusterNodes is null)
            {
                return new List<RedisServerNode>();
            }

            return clusterNodes.Nodes
                .Select(node =>
                {
                    var (host, port) = ResolveEndpoint(node.EndPoint);
                    var role = node.IsReplica ? "slave" : "master";
                    return new RedisServerNode(host, port, role);
                })
                .ToList();
        }

        private static (string Host, int Port) ResolveEndpoint(EndPoint endpoint)
        {
            return endpoint switch
            {
                DnsEndPoint dnsEndPoint => (dnsEndPoint.Host, dnsEndPoint.Port),
                IPEndPoint ipEndPoint => (ipEndPoint.Address.ToString(), ipEndPoint.Port),
                _ => (endpoint.ToString() ?? string.Empty, 0)
            };
        }
    }
}
