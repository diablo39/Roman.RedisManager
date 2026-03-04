using Roman.RedisManager.Domain.Repositories;

namespace Roman.RedisManager.Application.CQRS.RedisDataTypes
{
    public class GetSortedSetRangeQuery
    {
        public Guid GroupId { get; set; }
        public string Key { get; set; } = string.Empty;
        public long Start { get; set; } = 0;
        public long Stop { get; set; } = -1;
    }

    public record GetSortedSetRangeQueryResult(IReadOnlyCollection<SortedSetEntryDto> Entries);

    public static class GetSortedSetRangeQueryHandler
    {
        public static async Task<GetSortedSetRangeQueryResult> Handle(
            GetSortedSetRangeQuery query,
            IRedisSortedSetRepository repository)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentNullException.ThrowIfNull(repository);

            var entries = await repository.GetSortedSetRangeAsync(
                query.GroupId,
                query.Key,
                query.Start,
                query.Stop).ConfigureAwait(false);

            var dtos = entries
                .Select(e => new SortedSetEntryDto(e.Member, e.Score))
                .ToList();

            return new GetSortedSetRangeQueryResult(dtos);
        }
    }
}
