using Microsoft.Extensions.Configuration;
using Roman.RedisManager.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Roman.RedisManager.Domain.Configuration
{
    public class RedisServerGroupConfiguration : IValidatableObject
    {
        [Required]
        [ConfigurationKeyName("Id")]
        public required Guid Id { get; set; }

        [Required]
        [MinLength(1)]
        [ConfigurationKeyName("Name")]
        public required string Name { get; set; }

        [Required]
        [MinLength(1)]
        [ConfigurationKeyName("ConnectionString")]
        public required string ConnectionString { get; set; }

        [Required]
        [ConfigurationKeyName("GroupType")]
        public required GroupType GroupType { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Id == Guid.Empty)
            {
                yield return new ValidationResult("Id must be a non-empty GUID.", new[] { nameof(Id) });
            }
        }
    }
}
