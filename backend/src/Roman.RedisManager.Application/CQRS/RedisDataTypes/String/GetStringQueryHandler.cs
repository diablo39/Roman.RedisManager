using Roman.RedisManager.Domain.Repositories.RedisDataTypes;

namespace Roman.RedisManager.Application.CQRS.RedisDataTypes.String
{
    public class GetStringQuery
    {
        public Guid GroupId { get; set; }
        public string Key { get; set; } = string.Empty;
    }

    public record GetStringQueryResult(string? Value);

    public static class GetStringQueryHandler
    {
        public static async Task<GetStringQueryResult> Handle(
            GetStringQuery query,
            IRedisStringRepository repository)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentNullException.ThrowIfNull(repository);

            var value = await repository.StringGetAsync(query.GroupId, query.Key).ConfigureAwait(false);

            return new GetStringQueryResult(value);
        }
    }
}
