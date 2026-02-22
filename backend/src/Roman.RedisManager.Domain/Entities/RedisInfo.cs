using System.Collections.Generic;

namespace Roman.RedisManager.Domain.Entities
{
    /// <summary>
    /// Represents the structured result of the Redis INFO command.
    /// Sections map section names (e.g. "Server", "Clients") to a
    /// dictionary of key/value pairs contained in that section.
    /// </summary>
    public record RedisInfo(
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Sections);
}
