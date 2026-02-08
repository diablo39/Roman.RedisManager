using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Roman.RedisManager.Infrastructure.Configuration
{
    public class RedisConfiguration
    {
        public const string SectionName = "Redis";

        [Required]
        [MinLength(1)]
        [ConfigurationKeyName("ServerGroups")]
        public required IEnumerable<RedisServerGroupConfiguration> ServerGroups { get; set; }
    }
}
