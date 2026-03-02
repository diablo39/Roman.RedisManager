using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;

namespace Roman.RedisManager.Application.CQRS
{
    public class GetKeyValueQuery
    {
        public Guid GroupId { get; set; }
        public string Key { get; set; } = string.Empty;
    }

    public record SortedSetEntryDto(string Member, double Score);

    public record GetKeyValueQueryResult(
        string Type,
        string? StringValue,
        IReadOnlyCollection<string>? ListValues,
        IReadOnlyCollection<string>? SetMembers,
        IReadOnlyDictionary<string, string>? HashFields,
        IReadOnlyCollection<SortedSetEntryDto>? SortedSetEntries);

    public static class GetKeyValueQueryHandler
    {
        public static async Task<GetKeyValueQueryResult> Handle(
            GetKeyValueQuery query,
            IRedisKeyRepository repository)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentNullException.ThrowIfNull(repository);

            var keyValue = await repository.GetKeyValueAsync(query.GroupId, query.Key).ConfigureAwait(false);

            var sortedSetDtos = keyValue.SortedSetEntries?
                .Select(e => new SortedSetEntryDto(e.Member, e.Score))
                .ToList();

            var hashFields = keyValue.HashFields != null
                ? (IReadOnlyDictionary<string, string>)keyValue.HashFields
                : null;

            return new GetKeyValueQueryResult(
                keyValue.Type.ToString(),
                keyValue.StringValue,
                keyValue.ListValues?.ToList(),
                keyValue.SetMembers?.ToList(),
                hashFields,
                sortedSetDtos);
        }
    }
}
