using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Roman.RedisManager.Web.Configuration
{
    public class RedisConfiguration
    {
        public const string SectionName = "Redis";

        [Required]
        [MinLength(1)]
        [ConfigurationKeyName("Servers")]
        public required IEnumerable<RedisServerConfiguration> Servers { get; set; }
    }
}
