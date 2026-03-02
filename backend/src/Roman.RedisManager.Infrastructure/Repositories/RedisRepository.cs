using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Infrastructure.Exceptions;
using Roman.RedisManager.Infrastructure.Redis;
using StackExchange.Redis;
using System.Net;
using System.Net.Sockets;
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

        public async Task<RedisSearchResult> SearchForKeysAsync(
            Guid groupId,
            string pattern,
            string cursor,
            int pageSize)
        {
            if (groupId == Guid.Empty)
            {
                throw new ArgumentException("Group identifier is required.", nameof(groupId));
            }

            if (pattern is null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            if (pageSize <= 0)
            {
                throw new ArgumentException("Page size must be a positive integer.", nameof(pageSize));
            }

            var group = _redisConfiguration.Value.ResolveServerGroup(groupId);

            try
            {
                var connection = await _connectionManager.GetConnectionAsync(groupId).ConfigureAwait(false);

                var effectivePattern = string.IsNullOrEmpty(pattern) ? "*" : pattern;

                // determine behaviour based on group type
                if (group.GroupType == Domain.Entities.GroupType.Cluster)
                {
                    // perform scan across all master nodes in sequence, using a composite cursor string
                    var masters = connection.GetServers()
                        .Where(s => s != null && s.EndPoint != null && !s.IsReplica)
                        .ToList();

                    // parse incoming cursor of form "nodeIndex:innerCursor" or just number
                    int nodeIndex = 0;
                    long innerCursor = 0;
                    if (!string.IsNullOrEmpty(cursor))
                    {
                        var parts = cursor.Split(':');
                        if (parts.Length >= 1 && int.TryParse(parts[0], out var ni))
                        {
                            nodeIndex = ni;
                        }
                        if (parts.Length == 2 && long.TryParse(parts[1], out var ic))
                        {
                            innerCursor = ic;
                        }
                    }

                    var collected = new List<RedisKey>();
                    var nextNodeIndex = nodeIndex;
                    long nextInner = innerCursor;
                    var nodeCursors = new Dictionary<string, long>(StringComparer.Ordinal);

                    while (collected.Count < pageSize && nextNodeIndex < masters.Count)
                    {
                        var server = masters[nextNodeIndex];
                        var scanResult = await server.ExecuteAsync(
                            "SCAN",
                            nextInner.ToString(),
                            "MATCH",
                            effectivePattern,
                            "COUNT",
                            pageSize.ToString()).ConfigureAwait(false);

                        var inner = (RedisResult[])scanResult!;
                        nextInner = long.Parse((string)inner[0]!);
                        var rawKeys = (RedisResult[])inner[1]!;

                        foreach (var k in rawKeys)
                        {
                            collected.Add(new RedisKey((string)k!));
                            if (collected.Count >= pageSize)
                                break;
                        }

                        // register the cursor for this node
                        var endpoint = server.EndPoint;
                        var (host, port) = ResolveEndpoint(endpoint);
                        var key = host + ":" + port;
                        nodeCursors[key] = nextInner;

                        if (nextInner == 0)
                        {
                            // finished this node, move to next
                            nextNodeIndex++;
                            nextInner = 0;
                        }
                        else
                        {
                            // still working on this node; stop scanning further
                            break;
                        }
                    }

                    // build the composite cursor string for the caller
                    string compositeCursor;
                    if (nextNodeIndex < masters.Count)
                    {
                        compositeCursor = nextNodeIndex + ":" + nextInner;
                    }
                    else
                    {
                        compositeCursor = "0";
                    }

                    var result = new RedisSearchResult(collected, 0)
                    {
                        NodeCursors = nodeCursors
                    };
                    return result;
                }
                else
                {
                    // standalone or single-instance behaviour stays the same; cursor must be numeric
                    if (!long.TryParse(cursor, out var numericCursor))
                    {
                        numericCursor = 0;
                    }

                    var server = GetServer(connection);
                    var scanResult = await server.ExecuteAsync(
                        "SCAN",
                        numericCursor.ToString(),
                        "MATCH",
                        effectivePattern,
                        "COUNT",
                        pageSize.ToString()).ConfigureAwait(false);

                    var inner = (RedisResult[])scanResult!;
                    var nextCursor = long.Parse((string)inner[0]!);
                    var rawKeys = (RedisResult[])inner[1]!;
                    var keys = rawKeys.Select(k => new RedisKey((string)k!)).ToList();
                    return new RedisSearchResult(keys, nextCursor);
                }
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

        public async Task<IReadOnlyCollection<RedisServerNode>> GetServerNodesAsync(Guid groupId)
        {
            if (groupId == Guid.Empty)
            {
                throw new ArgumentException("Group identifier is required.", nameof(groupId));
            }

            // validate that the group exists
            var group = _redisConfiguration.Value.ResolveServerGroup(groupId);

            try
            {
                var connection = await _connectionManager.GetConnectionAsync(groupId).ConfigureAwait(false);

                var nodes = new List<RedisServerNode>();
                foreach (var candidate in connection.GetServers())
                {
                    if(candidate == null || candidate.EndPoint == null)
                    {
                        continue;
                    }
                    
                    // for cluster groups, skip unspecified endpoints
                    if (group.GroupType == Domain.Entities.GroupType.Cluster &&
                        candidate.EndPoint.AddressFamily == AddressFamily.Unspecified)
                    {
                        continue;
                    }

                    string role = candidate.IsReplica ? "slave" : "master";
                    var (host, port) = ResolveEndpoint(candidate.EndPoint);
                    nodes.Add(new RedisServerNode(host, port, role));
                }

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

        public async Task<RedisInfo> GetInfoAsync(Guid groupId, string host, int port)
        {
            if (groupId == Guid.Empty)
            {
                throw new ArgumentException("Group identifier is required.", nameof(groupId));
            }

            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException("Host is required.", nameof(host));
            }

            if (port <= 0)
            {
                throw new ArgumentException("Port must be a positive integer.", nameof(port));
            }

            // ensure group exists; we don't actually use connection string here but validation is important
            _ = _redisConfiguration.Value.ResolveServerGroup(groupId);

            try
            {
                var connection = await _connectionManager.GetConnectionAsync(groupId).ConfigureAwait(false);

                var server = connection.GetServer(host, port);

                // Use strongly-typed InfoAsync rather than raw execution
                var infoSections = await server.InfoAsync().ConfigureAwait(false);
                var sections = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

                foreach (var sec in infoSections)
                {
                    var fieldDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var kvp in sec)
                    {
                        fieldDict[kvp.Key] = kvp.Value;
                    }

                    sections[sec.Key] = fieldDict;
                }

                return new RedisInfo(sections);
            }
            catch (RedisConnectionException ex)
            {
                throw new RedisConnectionFailureException("Failed to execute INFO command for the requested instance.", ex);
            }
            catch (RedisTimeoutException ex)
            {
                throw new RedisConnectionFailureException("Timed out while executing INFO command.", ex);
            }
            catch (RedisServerException ex)
            {
                throw new RedisConnectionFailureException("Redis server returned an error while executing INFO.", ex);
            }
        }


        private static IServer GetServer(IConnectionMultiplexer connection)
        {
            var endpoint = connection.GetEndPoints().FirstOrDefault()
                ?? throw new InvalidOperationException("Redis connection does not expose endpoints.");

            return connection.GetServer(endpoint);
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
