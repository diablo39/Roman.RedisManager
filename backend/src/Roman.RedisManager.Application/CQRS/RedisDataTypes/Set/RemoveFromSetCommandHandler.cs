using Roman.RedisManager.Domain.Repositories.RedisDataTypes;

namespace Roman.RedisManager.Application.CQRS.RedisDataTypes.Set
{
    public class RemoveFromSetCommand
    {
        public Guid GroupId { get; set; }
        public string Key { get; set; } = string.Empty;
        public IReadOnlyCollection<string> Members { get; set; } = Array.Empty<string>();
    }

    public record RemoveFromSetCommandResult(long RemovedCount);

    public static class RemoveFromSetCommandHandler
    {
        public static async Task<RemoveFromSetCommandResult> Handle(
            RemoveFromSetCommand command,
            IRedisSetRepository repository)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(repository);

            var removed = await repository.SetRemoveAsync(
                command.GroupId,
                command.Key,
                command.Members).ConfigureAwait(false);

            return new RemoveFromSetCommandResult(removed);
        }
    }
}
