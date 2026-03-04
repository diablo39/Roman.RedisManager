using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Repositories;

namespace Roman.RedisManager.Application.CQRS.RedisDataTypes
{
    public class AddToSortedSetCommand
    {
        public Guid GroupId { get; set; }
        public string Key { get; set; } = string.Empty;
        public IReadOnlyCollection<RedisSortedSetEntry> Entries { get; set; } = Array.Empty<RedisSortedSetEntry>();
        public TimeSpan? Ttl { get; set; }
    }

    public record AddToSortedSetCommandResult(bool Success);

    public static class AddToSortedSetCommandHandler
    {
        public static async Task<AddToSortedSetCommandResult> Handle(
            AddToSortedSetCommand command,
            IRedisSortedSetRepository repository)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(repository);

            await repository.AddToSortedSetAsync(
                command.GroupId,
                command.Key,
                command.Entries,
                command.Ttl).ConfigureAwait(false);

            return new AddToSortedSetCommandResult(true);
        }
    }
}
