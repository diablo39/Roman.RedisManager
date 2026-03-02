using Roman.RedisManager.Domain.Repositories;

namespace Roman.RedisManager.Application.CQRS
{
    public class SetHashFieldsCommand
    {
        public Guid GroupId { get; set; }
        public string Key { get; set; } = string.Empty;
        public IReadOnlyDictionary<string, string> Fields { get; set; } = new Dictionary<string, string>();
        public TimeSpan? Ttl { get; set; }
    }

    public record SetHashFieldsCommandResult(bool Success);

    public static class SetHashFieldsCommandHandler
    {
        public static async Task<SetHashFieldsCommandResult> Handle(
            SetHashFieldsCommand command,
            IRedisHashRepository repository)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(repository);

            await repository.SetHashFieldsAsync(
                command.GroupId,
                command.Key,
                command.Fields,
                command.Ttl).ConfigureAwait(false);

            return new SetHashFieldsCommandResult(true);
        }
    }
}
