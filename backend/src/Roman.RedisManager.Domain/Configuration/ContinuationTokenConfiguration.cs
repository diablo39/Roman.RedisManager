using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Roman.RedisManager.Domain.Configuration
{
    public class ContinuationTokenConfiguration
    {
        public const string SectionName = "ContinuationToken";

        [Required]
        [MinLength(16)]
        [ConfigurationKeyName("TokenSecret")]
        public required string TokenSecret { get; set; }

        [ConfigurationKeyName("TokenTtlMinutes")]
        public int? TokenTtlMinutes { get; set; }
    }
}
