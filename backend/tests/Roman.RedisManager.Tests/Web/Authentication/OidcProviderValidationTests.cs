using Roman.RedisManager.Domain.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Roman.RedisManager.Tests.Web.Authentication
{
    public class OidcProviderValidationTests
    {
        [Fact]
        public void Validate_NoEnabledProviders_IsAllowed()
        {
            // Arrange
            var configuration = new OidcAuthenticationConfiguration
            {
                Providers = new[]
                {
                    new OidcProviderConfiguration
                    {
                        ProviderKey = "entra",
                        DisplayName = "Entra",
                        Enabled = false,
                        Kind = OidcProviderKind.EntraId,
                        Authority = "https://login.microsoftonline.com/common/v2.0",
                        ClientId = "client-id"
                    }
                }
            };

            var validationContext = new ValidationContext(configuration);
            var validationResults = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(configuration, validationContext, validationResults, validateAllProperties: true);

            // Assert
            isValid.ShouldBeTrue();
            validationResults.ShouldBeEmpty();
        }
    }
}
