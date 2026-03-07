using System.ComponentModel.DataAnnotations;
using Roman.RedisManager.Domain.Configuration;

namespace Roman.RedisManager.Tests.Domain.Configuration
{
    public class RedisSearchLimitsConfigurationTests
    {
        [Fact]
        public void MaxPageSize_WhenLessThanOne_FailsValidation()
        {
            // Arrange
            var config = new RedisSearchLimitsConfiguration
            {
                MaxPageSize = 0
            };

            var context = new ValidationContext(config);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(config, context, results, validateAllProperties: true);

            // Assert
            isValid.ShouldBeFalse();
            results.ShouldContain(result => result.MemberNames.Contains(nameof(RedisSearchLimitsConfiguration.MaxPageSize)));
        }

        [Fact]
        public void MaxPageSize_WhenPositive_PassesValidation()
        {
            // Arrange
            var config = new RedisSearchLimitsConfiguration
            {
                MaxPageSize = 200
            };

            var context = new ValidationContext(config);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(config, context, results, validateAllProperties: true);

            // Assert
            isValid.ShouldBeTrue();
            results.ShouldBeEmpty();
        }
    }
}
