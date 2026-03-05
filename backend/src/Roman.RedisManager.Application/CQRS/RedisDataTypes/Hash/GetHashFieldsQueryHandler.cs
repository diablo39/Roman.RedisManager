using Roman.RedisManager.Domain.Repositories.RedisDataTypes;

namespace Roman.RedisManager.Application.CQRS.RedisDataTypes.Hash
{
    public class GetHashFieldsQuery
    {
        public Guid GroupId { get; set; }
        public string Key { get; set; } = string.Empty;
        public long Cursor { get; set; } = 0;
        public int PageSize { get; set; } = 100;
    }

    public record HashFieldDto(string Field, string Value);

    public record GetHashFieldsQueryResult(
        IReadOnlyCollection<HashFieldDto> Fields,
        long Cursor,
        bool HasMoreResults);

    public static class GetHashFieldsQueryHandler
    {
        public static async Task<GetHashFieldsQueryResult> Handle(
            GetHashFieldsQuery query,
            IRedisHashRepository repository)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentNullException.ThrowIfNull(repository);

            var scanResult = await repository.HashScanAsync(
                query.GroupId,
                query.Key,
                query.Cursor,
                query.PageSize).ConfigureAwait(false);

            var fields = scanResult.Items
                .Select(e => new HashFieldDto(e.Field, e.Value))
                .ToList();

            return new GetHashFieldsQueryResult(fields, scanResult.Cursor, scanResult.HasMoreResults);
        }
    }
}
