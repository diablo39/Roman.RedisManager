using Roman.RedisManager.Domain.Repositories.RedisDataTypes;

namespace Roman.RedisManager.Application.CQRS.RedisDataTypes.SortedSet
{
    public class RemoveFromSortedSetCommand
    {
        public Guid GroupId { get; set; }
        public string Key { get; set; } = string.Empty;
        public IReadOnlyCollection<string> Members { get; set; } = Array.Empty<string>();
    }

    public record RemoveFromSortedSetCommandResult(long RemovedCount);

    public static class RemoveFromSortedSetCommandHandler
    {
        public static async Task<RemoveFromSortedSetCommandResult> Handle(
            RemoveFromSortedSetCommand command,
            IRedisSortedSetRepository repository)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(repository);

            var removed = await repository.RemoveFromSortedSetAsync(
                command.GroupId,
                command.Key,
                command.Members).ConfigureAwait(false);

            return new RemoveFromSortedSetCommandResult(removed);
        }
    }
}
