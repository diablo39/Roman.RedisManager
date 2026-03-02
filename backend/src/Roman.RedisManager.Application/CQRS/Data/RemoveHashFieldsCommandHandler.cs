using Roman.RedisManager.Domain.Repositories;

namespace Roman.RedisManager.Application.CQRS.Data
{
    public class RemoveHashFieldsCommand
    {
        public Guid GroupId { get; set; }
        public string Key { get; set; } = string.Empty;
        public IReadOnlyCollection<string> Fields { get; set; } = Array.Empty<string>();
    }

    public record RemoveHashFieldsCommandResult(long RemovedCount);

    public static class RemoveHashFieldsCommandHandler
    {
        public static async Task<RemoveHashFieldsCommandResult> Handle(
            RemoveHashFieldsCommand command,
            IRedisHashRepository repository)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(repository);

            var removed = await repository.RemoveHashFieldsAsync(
                command.GroupId,
                command.Key,
                command.Fields).ConfigureAwait(false);

            return new RemoveHashFieldsCommandResult(removed);
        }
    }
}
