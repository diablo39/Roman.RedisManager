using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Repositories;

namespace Roman.RedisManager.Application.CQRS.Data
{
    public class SetStringCommand
    {
        public Guid GroupId { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public TimeSpan? Ttl { get; set; }
        public SetCondition Condition { get; set; } = SetCondition.None;
    }

    public record SetStringCommandResult(bool Success);

    public static class SetStringCommandHandler
    {
        public static async Task<SetStringCommandResult> Handle(
            SetStringCommand command,
            IRedisStringRepository repository)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(repository);

            var success = await repository.StringSetAsync(
                command.GroupId,
                command.Key,
                command.Value,
                command.Ttl,
                command.Condition).ConfigureAwait(false);

            return new SetStringCommandResult(success);
        }
    }
}
