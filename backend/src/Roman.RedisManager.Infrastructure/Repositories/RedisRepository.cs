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
            var sections = await server.InfoAsync("replication").ConfigureAwait(false);
            var replicationSection = sections.FirstOrDefault();

            if (replicationSection is null)
            {
                return new List<RedisServerNode>();
            }

            // Check if the current server is a slave
            var roleEntry = replicationSection.FirstOrDefault(kvp => kvp.Key.Equals("role", StringComparison.OrdinalIgnoreCase));
            var isSlaveNode = roleEntry.Value?.Equals("slave", StringComparison.OrdinalIgnoreCase) ?? false;

            // If it's a slave, find and connect to the master
            if (isSlaveNode)
            {
                var masterHost = replicationSection.FirstOrDefault(kvp => kvp.Key.Equals("master_host", StringComparison.OrdinalIgnoreCase)).Value;
                var masterPortString = replicationSection.FirstOrDefault(kvp => kvp.Key.Equals("master_port", StringComparison.OrdinalIgnoreCase)).Value;

                if (!string.IsNullOrWhiteSpace(masterHost) && 
                    int.TryParse(masterPortString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var masterPort))
                {
                    try
                    {
                        var masterConnection = await ConnectionMultiplexer.ConnectAsync($"{masterHost}:{masterPort}").ConfigureAwait(false);
                        try
                        {
                            var masterServer = GetServer(masterConnection);
                            var masterNodes = await ReadStandaloneNodesAsync(masterServer).ConfigureAwait(false);
                            return masterNodes;
                        }
                        finally
                        {
                            await masterConnection.CloseAsync().ConfigureAwait(false);
                            masterConnection.Dispose();
                        }
                    }
                    catch
                    {
                        // If we can't connect to master, continue with the slave information
                    }
                }
            }

            // Original logic for master node
            var nodes = new List<RedisServerNode>();
            var endpoint = server.EndPoint;
            if (endpoint is not null)
            {
                var (host, port) = ResolveEndpoint(endpoint);
                nodes.Add(new RedisServerNode(host, port, "master"));
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
                .Where(node => node.EndPoint is not null)
                .Select(node =>
                {
                    var (host, port) = ResolveEndpoint(node.EndPoint!);
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
