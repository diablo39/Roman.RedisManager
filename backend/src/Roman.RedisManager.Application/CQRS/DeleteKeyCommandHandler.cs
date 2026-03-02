using Roman.RedisManager.Domain.Repositories;

namespace Roman.RedisManager.Application.CQRS
{
    public class DeleteKeyCommand
    {
        public Guid GroupId { get; set; }
        public string Key { get; set; } = string.Empty;
    }

    public record DeleteKeyCommandResult(bool Deleted);

    public static class DeleteKeyCommandHandler
    {
        public static async Task<DeleteKeyCommandResult> Handle(
            DeleteKeyCommand command,
            IRedisKeyRepository repository)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(repository);

            var deleted = await repository.DeleteKeyAsync(command.GroupId, command.Key).ConfigureAwait(false);

            return new DeleteKeyCommandResult(deleted);
        }
    }
}
