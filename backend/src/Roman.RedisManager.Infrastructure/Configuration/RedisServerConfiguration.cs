using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Roman.RedisManager.Infrastructure.Configuration
{
    public class RedisServerConfiguration
    {
        [Required]
        [MinLength(1)]
        [ConfigurationKeyName("Name")]
        public required string Name { get; set; }

        [Required]
        [MinLength(1)]
        [ConfigurationKeyName("Endpoints")]
        public required IEnumerable<string> Endpoints { get; set; }

        [ConfigurationKeyName("Password")]
        public string? Password { get; set; }

        [ConfigurationKeyName("User")]
        public string? User { get; set; }
    }
}
