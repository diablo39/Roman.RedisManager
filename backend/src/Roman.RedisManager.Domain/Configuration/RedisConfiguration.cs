using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Roman.RedisManager.Domain.Configuration
{
    public class RedisConfiguration
    {
        public const string SectionName = "Redis";

        [Required]
        [ConfigurationKeyName("Search")]
        public RedisSearchLimitsConfiguration Search { get; set; } = new();

        [Required]
        [MinLength(1)]
        [ConfigurationKeyName("ServerGroups")]
        public required IEnumerable<RedisServerGroupConfiguration> ServerGroups { get; set; }

        /// <summary>
        /// Resolves a <see cref="RedisServerGroupConfiguration"/> by its group identifier.
        /// </summary>
        /// <param name="groupId">The identifier of the server group to resolve.</param>
        /// <returns>The matching <see cref="RedisServerGroupConfiguration"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="groupId"/> is empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the Redis server group configuration is unavailable.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when no server group matches the provided <paramref name="groupId"/>.</exception>
        public RedisServerGroupConfiguration ResolveServerGroup(Guid groupId)
        {
            if (groupId == Guid.Empty)
            {
                throw new ArgumentException("Group identifier is required.", nameof(groupId));
            }

            var configuration = ServerGroups ?? throw new InvalidOperationException("Redis configuration is unavailable.");

            var serverGroup = configuration.FirstOrDefault(group =>
                group.Id == groupId);

            if (serverGroup is null)
            {
                throw new KeyNotFoundException($"Redis server group with id '{groupId}' was not found.");
            }

            return serverGroup;
        }
    }
}
