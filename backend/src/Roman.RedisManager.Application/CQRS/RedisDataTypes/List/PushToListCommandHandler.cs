using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Repositories.RedisDataTypes;

namespace Roman.RedisManager.Application.CQRS.RedisDataTypes.List
{
    public class PushToListCommand
    {
        public Guid GroupId { get; set; }
        public string Key { get; set; } = string.Empty;
        public IReadOnlyCollection<string> Values { get; set; } = Array.Empty<string>();
        public ListDirection Direction { get; set; } = ListDirection.Right;
        public TimeSpan? Ttl { get; set; }
    }

    public record PushToListCommandResult(bool Success);

    public static class PushToListCommandHandler
    {
        public static async Task<PushToListCommandResult> Handle(
            PushToListCommand command,
            IRedisListRepository repository)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(repository);

            await repository.ListPushAsync(
                command.GroupId,
                command.Key,
                command.Values,
                command.Direction,
                command.Ttl).ConfigureAwait(false);

            return new PushToListCommandResult(true);
        }
    }
}
