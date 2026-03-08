using Microsoft.Extensions.Logging;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Web.Authorization;
using System.Security.Claims;

namespace Roman.RedisManager.Tests.Web.Authorization
{
    public class AuthorizationDecisionLoggingTests
    {
        [Fact]
        public void LogDecision_WithDecisionContext_WritesStructuredFields()
        {
            // Arrange
            var logger = new CapturingLogger<AuthorizationDecisionLogger>();
            var decisionLogger = new AuthorizationDecisionLogger(logger);
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-1"),
                new Claim(ClaimTypes.Role, "admin"),
                new Claim("provider", "entra")
            }, "test"));

            // Act
            decisionLogger.LogDecision(principal, PermissionAction.DeleteKey, Guid.Parse("11111111-1111-1111-1111-111111111111"), true, AuthorizationDecisionReason.AllowByGroupOverride);

            // Assert
            logger.Messages.ShouldNotBeEmpty();
            logger.Messages.ShouldContain(message => message.Contains("provider=entra", StringComparison.OrdinalIgnoreCase));
            logger.Messages.ShouldContain(message => message.Contains("action=DeleteKey", StringComparison.OrdinalIgnoreCase));
            logger.Messages.ShouldContain(message => message.Contains("reason=AllowByGroupOverride", StringComparison.OrdinalIgnoreCase));
        }

        private sealed class CapturingLogger<T> : ILogger<T>
        {
            public List<string> Messages { get; } = new();

            IDisposable ILogger.BeginScope<TState>(TState state) => NullScope.Instance;

            bool ILogger.IsEnabled(LogLevel logLevel) => true;

            void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                Messages.Add(formatter(state, exception));
            }

            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();

                public void Dispose()
                {
                }
            }
        }
    }
}
