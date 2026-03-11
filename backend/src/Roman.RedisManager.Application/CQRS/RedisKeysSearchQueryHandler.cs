using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Roman.RedisManager.Application.CQRS
{
    public class RedisKeysSearchQuery
    {
        public Guid GroupId { get; set; }
        public string Pattern { get; set; } = "*";
        public string? ContinuationToken { get; set; }
        public int PageSize { get; set; } = 100;
    }

    public record RedisKeyDto(
        string Key,
        string Type,
        long? TtlMilliseconds,
        bool HasExpiration);

    public record RedisKeysSearchQueryResult(
        IReadOnlyCollection<RedisKeyDto> Keys,
        bool HasMoreResults,
        string? ContinuationToken);

    public static class RedisKeysSearchQueryHandler
    {
        public static async Task<RedisKeysSearchQueryResult> Handle(
            RedisKeysSearchQuery query,
            IRedisRepository repository,
            IOptions<RedisSearchLimitsConfiguration> limitsOptions)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(limitsOptions);

            var maxPageSize = limitsOptions.Value.MaxPageSize;
            var requestedPageSize = query.PageSize <= 0 ? maxPageSize : query.PageSize;
            var effectivePageSize = Math.Min(requestedPageSize, maxPageSize);

            var result = await repository.SearchForKeysAsync(
                query.GroupId,
                query.Pattern,
                query.ContinuationToken,
                effectivePageSize).ConfigureAwait(false);

            var dtos = result.Keys
                .Select(k => new RedisKeyDto(
                    k.Key,
                    k.Type.ToString(),
                    k.Ttl.HasValue ? (long)k.Ttl.Value.TotalMilliseconds : null,
                    k.HasExpiration))
                .ToList();

            var nextToken = result.HasMoreResults ? result.ContinuationToken : null;

            return new RedisKeysSearchQueryResult(dtos, result.HasMoreResults, nextToken);
        }
    }
}
