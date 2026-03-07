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

    public record RedisKeyDto(string Key);

    public record RedisKeysSearchQueryResult(
        IReadOnlyCollection<RedisKeyDto> Keys,
        bool HasMoreResults,
        string? ContinuationToken);

    public static class RedisKeysSearchQueryHandler
    {
        public static async Task<RedisKeysSearchQueryResult> Handle(
            RedisKeysSearchQuery query,
            IRedisRepository repository)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentNullException.ThrowIfNull(repository);

            var result = await repository.SearchForKeysAsync(
                query.GroupId,
                query.Pattern,
                query.ContinuationToken,
                query.PageSize).ConfigureAwait(false);

            var dtos = result.Keys
                .Select(k => new RedisKeyDto(k.Key))
                .ToList();

            var nextToken = result.HasMoreResults ? result.ContinuationToken : null;

            return new RedisKeysSearchQueryResult(dtos, result.HasMoreResults, nextToken);
        }
    }
}
