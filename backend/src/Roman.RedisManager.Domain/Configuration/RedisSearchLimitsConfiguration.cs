using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Roman.RedisManager.Domain.Configuration
{
    public class RedisSearchLimitsConfiguration
    {
        public const string SectionName = "Search";

        [Required]
        [Range(1, int.MaxValue)]
        [ConfigurationKeyName("MaxPageSize")]
        public int MaxPageSize { get; set; }
    }
}
