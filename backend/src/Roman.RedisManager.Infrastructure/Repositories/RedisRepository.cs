using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Infrastructure.Exceptions;
using Roman.RedisManager.Infrastructure.Redis;
using StackExchange.Redis;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using RedisKey = Roman.RedisManager.Domain.Entities.RedisKey;
using Roman.RedisManager.Domain.Entities.Server;

namespace Roman.RedisManager.Infrastructure.Repositories
{
    public class RedisRepository : IRedisRepository
    {
        private readonly IRedisConnectionManager _connectionManager;
        private readonly IOptions<RedisConfiguration> _redisConfiguration;
        private readonly IContinuationTokenCodec _continuationTokenCodec;
        private readonly IOptions<ContinuationTokenConfiguration> _continuationTokenConfiguration;

        public RedisRepository(
            IRedisConnectionManager connectionManager,
            IOptions<RedisConfiguration> redisConfiguration,
            IContinuationTokenCodec continuationTokenCodec,
            IOptions<ContinuationTokenConfiguration> continuationTokenConfiguration)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _redisConfiguration = redisConfiguration ?? throw new ArgumentNullException(nameof(redisConfiguration));
            _continuationTokenCodec = continuationTokenCodec ?? throw new ArgumentNullException(nameof(continuationTokenCodec));
            _continuationTokenConfiguration = continuationTokenConfiguration ?? throw new ArgumentNullException(nameof(continuationTokenConfiguration));
        }

        public async Task<RedisSearchResult> SearchForKeysAsync(
            Guid groupId,
            string pattern,
            string? continuationToken,
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
                var contextHash = ComputeContextHash(groupId, effectivePattern, pageSize);

                ContinuationTokenEnvelope? envelope = null;
                if (!string.IsNullOrWhiteSpace(continuationToken))
                {
                    envelope = _continuationTokenCodec.Decode(continuationToken);
                    ValidateEnvelope(envelope, contextHash, group.GroupType);
                }

                if (group.GroupType == GroupType.Cluster)
                {
                    var masters = connection.GetServers()
                        .Where(s => s != null && s.EndPoint != null && !s.IsReplica)
                        .ToList();

                    var nodeIndex = envelope?.ClusterState?.NodeIndex ?? 0;
                    var innerCursor = envelope?.ClusterState?.NodeCursor ?? 0;

                    var collected = new List<RedisKey>();
                    var nextNodeIndex = nodeIndex;
                    long nextInner = innerCursor;

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

                        if (nextInner == 0)
                        {
                            nextNodeIndex++;
                            nextInner = 0;
                        }
                        else
                        {
                            break;
                        }
                    }

                    var hasMore = nextNodeIndex < masters.Count;
                    string? nextContinuationToken = null;
                    if (hasMore)
                    {
                        var nextEnvelope = new ContinuationTokenEnvelope
                        {
                            Version = 1,
                            ContextHash = contextHash,
                            Mode = ContinuationTokenMode.Cluster,
                            IssuedAtUtc = DateTimeOffset.UtcNow,
                            ClusterState = new ClusterCursorState(nextNodeIndex, nextInner, BuildTopologyFingerprint(masters))
                        };

                        nextContinuationToken = _continuationTokenCodec.Encode(nextEnvelope);
                    }

                    var result = new RedisSearchResult(collected, hasMore)
                    {
                        ContinuationToken = nextContinuationToken
                    };
                    return result;
                }
                else
                {
                    var numericCursor = envelope?.StandaloneState?.Cursor ?? 0;

                    var server = GetServer(connection);
                    var keys = new List<RedisKey>();
                    var nextCursor = numericCursor;

                    // Avoid empty first-page responses that still require continuation when there are no matches.
                    do
                    {
                        var scanResult = await server.ExecuteAsync(
                            "SCAN",
                            nextCursor.ToString(),
                            "MATCH",
                            effectivePattern,
                            "COUNT",
                            pageSize.ToString()).ConfigureAwait(false);

                        var inner = (RedisResult[])scanResult!;
                        nextCursor = long.Parse((string)inner[0]!);
                        var rawKeys = (RedisResult[])inner[1]!;

                        foreach (var key in rawKeys)
                        {
                            keys.Add(new RedisKey((string)key!));
                            if (keys.Count >= pageSize)
                            {
                                break;
                            }
                        }
                    }
                    while (keys.Count == 0 && nextCursor != 0);

                    var hasMore = nextCursor != 0;
                    string? nextContinuationToken = null;
                    if (hasMore)
                    {
                        var nextEnvelope = new ContinuationTokenEnvelope
                        {
                            Version = 1,
                            ContextHash = contextHash,
                            Mode = ContinuationTokenMode.Standalone,
                            IssuedAtUtc = DateTimeOffset.UtcNow,
                            StandaloneState = new StandaloneCursorState(nextCursor)
                        };

                        nextContinuationToken = _continuationTokenCodec.Encode(nextEnvelope);
                    }

                    return new RedisSearchResult(keys, hasMore)
                    {
                        ContinuationToken = nextContinuationToken
                    };
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

        private void ValidateEnvelope(ContinuationTokenEnvelope envelope, string contextHash, GroupType groupType)
        {
            if (!string.Equals(envelope.ContextHash, contextHash, StringComparison.Ordinal))
            {
                throw new InvalidContinuationTokenException(
                    ContinuationTokenError.ContinuationContextMismatch,
                    "Continuation token does not match the current search parameters. Start a new search.");
            }

            if (groupType == GroupType.Cluster && envelope.Mode != ContinuationTokenMode.Cluster)
            {
                throw new InvalidContinuationTokenException(
                    ContinuationTokenError.ContinuationContextMismatch,
                    "Continuation token does not match the current search parameters. Start a new search.");
            }

            if (groupType == GroupType.Standalone && envelope.Mode != ContinuationTokenMode.Standalone)
            {
                throw new InvalidContinuationTokenException(
                    ContinuationTokenError.ContinuationContextMismatch,
                    "Continuation token does not match the current search parameters. Start a new search.");
            }

            if (groupType == GroupType.Cluster && envelope.ClusterState is null)
            {
                throw new InvalidContinuationTokenException(
                    ContinuationTokenError.InvalidContinuationToken,
                    "Continuation token is invalid. Start a new search.");
            }

            if (groupType == GroupType.Standalone && envelope.StandaloneState is null)
            {
                throw new InvalidContinuationTokenException(
                    ContinuationTokenError.InvalidContinuationToken,
                    "Continuation token is invalid. Start a new search.");
            }

            var tokenTtlMinutes = _continuationTokenConfiguration.Value.TokenTtlMinutes;
            if (tokenTtlMinutes.HasValue && envelope.IssuedAtUtc.AddMinutes(tokenTtlMinutes.Value) < DateTimeOffset.UtcNow)
            {
                throw new InvalidContinuationTokenException(
                    ContinuationTokenError.ContinuationNotResumable,
                    "Continuation token can no longer be resumed. Start a new search.");
            }
        }

        private static string BuildTopologyFingerprint(IReadOnlyCollection<IServer> masters)
        {
            var orderedEndpoints = masters
                .Select(server => ResolveEndpoint(server.EndPoint))
                .OrderBy(endpoint => endpoint.Host, StringComparer.Ordinal)
                .ThenBy(endpoint => endpoint.Port)
                .Select(endpoint => $"{endpoint.Host}:{endpoint.Port}");

            return string.Join("|", orderedEndpoints);
        }

        private static string ComputeContextHash(Guid groupId, string pattern, int pageSize)
        {
            var raw = $"{groupId:N}|{pattern}|{pageSize}";
            var bytes = Encoding.UTF8.GetBytes(raw);
            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
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
                    if (group.GroupType == GroupType.Cluster &&
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
