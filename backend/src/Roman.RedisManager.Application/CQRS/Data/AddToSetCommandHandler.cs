using Roman.RedisManager.Domain.Repositories;

namespace Roman.RedisManager.Application.CQRS.Data
{
    public class AddToSetCommand
    {
        public Guid GroupId { get; set; }
        public string Key { get; set; } = string.Empty;
        public IReadOnlyCollection<string> Members { get; set; } = Array.Empty<string>();
        public TimeSpan? Ttl { get; set; }
    }

    public record AddToSetCommandResult(bool Success);

    public static class AddToSetCommandHandler
    {
        public static async Task<AddToSetCommandResult> Handle(
            AddToSetCommand command,
            IRedisSetRepository repository)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(repository);

            await repository.SetAddAsync(
                command.GroupId,
                command.Key,
                command.Members,
                command.Ttl).ConfigureAwait(false);

            return new AddToSetCommandResult(true);
        }
    }
}
