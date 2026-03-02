using Roman.RedisManager.Domain.Repositories;

namespace Roman.RedisManager.Application.CQRS.Data
{
    public class GetSetMembersQuery
    {
        public Guid GroupId { get; set; }
        public string Key { get; set; } = string.Empty;
        public long Cursor { get; set; } = 0;
        public int PageSize { get; set; } = 100;
    }

    public record GetSetMembersQueryResult(
        IReadOnlyCollection<string> Members,
        long Cursor,
        bool HasMoreResults);

    public static class GetSetMembersQueryHandler
    {
        public static async Task<GetSetMembersQueryResult> Handle(
            GetSetMembersQuery query,
            IRedisSetRepository repository)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentNullException.ThrowIfNull(repository);

            var scanResult = await repository.SetScanAsync(
                query.GroupId,
                query.Key,
                query.Cursor,
                query.PageSize).ConfigureAwait(false);

            return new GetSetMembersQueryResult(
                scanResult.Items.ToList(),
                scanResult.Cursor,
                scanResult.HasMoreResults);
        }
    }
}
