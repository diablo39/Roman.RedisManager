using Microsoft.Extensions.Configuration;
using Roman.RedisManager.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Roman.RedisManager.Infrastructure.Configuration
{
    public class RedisServerGroupConfiguration
    {
        [Required]
        [MinLength(1)]
        [ConfigurationKeyName("Name")]
        public required string Name { get; set; }

        [Required]
        [MinLength(1)]
        [ConfigurationKeyName("Endpoints")]
        public required IEnumerable<string> Endpoints { get; set; }

        [Required]
        [ConfigurationKeyName("GroupType")]
        public required GroupType GroupType { get; set; }

 
    }
}
