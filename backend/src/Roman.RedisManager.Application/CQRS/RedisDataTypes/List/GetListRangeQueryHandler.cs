using Roman.RedisManager.Domain.Repositories.RedisDataTypes;

namespace Roman.RedisManager.Application.CQRS.RedisDataTypes.List
{
    public class GetListRangeQuery
    {
        public Guid GroupId { get; set; }
        public string Key { get; set; } = string.Empty;
        public long Start { get; set; } = 0;
        public long Stop { get; set; } = -1;
    }

    public record GetListRangeQueryResult(IReadOnlyCollection<string> Values);

    public static class GetListRangeQueryHandler
    {
        public static async Task<GetListRangeQueryResult> Handle(
            GetListRangeQuery query,
            IRedisListRepository repository)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentNullException.ThrowIfNull(repository);

            var values = await repository.ListRangeAsync(
                query.GroupId,
                query.Key,
                query.Start,
                query.Stop).ConfigureAwait(false);

            return new GetListRangeQueryResult(values);
        }
    }
}
