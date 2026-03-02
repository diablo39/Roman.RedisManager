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
        /// <summary>
        /// Composite cursor value. Clients should treat this as an opaque string;
        /// when scanning a cluster the repository will return values like
        /// "0:34" meaning page 34 of the first master node.
        /// </summary>
        public string Cursor { get; set; } = "0";
        public int PageSize { get; set; } = 100;
    }

    public record RedisKeyDto(string Key);

    public record RedisKeysSearchQueryResult(
        IReadOnlyCollection<RedisKeyDto> Keys,
        long Cursor,
        bool HasMoreResults)
    {
        public Dictionary<string,long>? NodeCursors { get; init; }
    };

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
                query.Cursor,
                query.PageSize).ConfigureAwait(false);

            var dtos = result.Keys
                .Select(k => new RedisKeyDto(k.Key))
                .ToList();

            return new RedisKeysSearchQueryResult(dtos, result.Cursor, result.HasMoreResults)
            {
                NodeCursors = result.NodeCursors
            };
        }
    }
}
