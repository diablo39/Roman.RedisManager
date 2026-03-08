using Roman.RedisManager.Domain.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Roman.RedisManager.Tests.Web.Authentication
{
    public class ProviderConfigurationExtensibilityTests
    {
        [Fact]
        public void Validate_WithAdditionalProviderDefinition_RemainsValid()
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
                        Enabled = true,
                        Kind = OidcProviderKind.EntraId,
                        Authority = "https://login.microsoftonline.com/common/v2.0",
                        ClientId = "entra-client-id"
                    },
                    new OidcProviderConfiguration
                    {
                        ProviderKey = "custom-provider",
                        DisplayName = "Custom Provider",
                        Enabled = true,
                        Kind = OidcProviderKind.GenericOidc,
                        Authority = "https://custom.idp.local",
                        ClientId = "custom-client-id"
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
