using Roman.RedisManager.Domain.Repositories.RedisDataTypes;

namespace Roman.RedisManager.Application.CQRS.RedisDataTypes.List
{
    public class RemoveFromListCommand
    {
        public Guid GroupId { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public long Count { get; set; } = 0;
    }

    public record RemoveFromListCommandResult(long RemovedCount);

    public static class RemoveFromListCommandHandler
    {
        public static async Task<RemoveFromListCommandResult> Handle(
            RemoveFromListCommand command,
            IRedisListRepository repository)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(repository);

            var removed = await repository.ListRemoveAsync(
                command.GroupId,
                command.Key,
                command.Value,
                command.Count).ConfigureAwait(false);

            return new RemoveFromListCommandResult(removed);
        }
    }
}
