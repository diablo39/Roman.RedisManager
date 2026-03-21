using Microsoft.Extensions.Logging;
using Roman.RedisManager.Web.Authorization;
using System.Security.Claims;

namespace Roman.RedisManager.Tests.Web.Authorization
{
    public class AuthorizationDecisionLoggingTests
    {
        [Fact]
        public void LogDecision_WithPolicyAndRoles_WritesStructuredFields()
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
            decisionLogger.LogDecision(principal, "Reader", true);

            // Assert
            logger.Messages.ShouldNotBeEmpty();
            logger.Messages.ShouldContain(message => message.Contains("provider=entra", StringComparison.OrdinalIgnoreCase));
            logger.Messages.ShouldContain(message => message.Contains("policy=Reader", StringComparison.OrdinalIgnoreCase));
            logger.Messages.ShouldContain(message => message.Contains("allowed=True", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void LogDecision_WithDeniedDecision_LogsDenyResult()
        {
            // Arrange
            var logger = new CapturingLogger<AuthorizationDecisionLogger>();
            var decisionLogger = new AuthorizationDecisionLogger(logger);
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-2"),
                new Claim(ClaimTypes.Role, "reader"),
                new Claim("provider", "google")
            }, "test"));

            // Act
            decisionLogger.LogDecision(principal, "Editor", false);

            // Assert
            logger.Messages.ShouldNotBeEmpty();
            logger.Messages.ShouldContain(message => message.Contains("policy=Editor", StringComparison.OrdinalIgnoreCase));
            logger.Messages.ShouldContain(message => message.Contains("allowed=False", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void LogDecision_WithNoNameIdentifier_LogsAnonymous()
        {
            // Arrange
            var logger = new CapturingLogger<AuthorizationDecisionLogger>();
            var decisionLogger = new AuthorizationDecisionLogger(logger);
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "reader")
            }, "test"));

            // Act
            decisionLogger.LogDecision(principal, "Reader", true);

            // Assert
            logger.Messages.ShouldNotBeEmpty();
            logger.Messages.ShouldContain(message => message.Contains("user=anonymous", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void LogDecision_WithNoProviderClaim_LogsUnknown()
        {
            // Arrange
            var logger = new CapturingLogger<AuthorizationDecisionLogger>();
            var decisionLogger = new AuthorizationDecisionLogger(logger);
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-3"),
                new Claim(ClaimTypes.Role, "admin")
            }, "test"));

            // Act
            decisionLogger.LogDecision(principal, "Editor", true);

            // Assert
            logger.Messages.ShouldNotBeEmpty();
            logger.Messages.ShouldContain(message => message.Contains("provider=unknown", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void LogDecision_LogsCorrectUserId()
        {
            // Arrange
            var logger = new CapturingLogger<AuthorizationDecisionLogger>();
            var decisionLogger = new AuthorizationDecisionLogger(logger);
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "specific-user-id"),
                new Claim(ClaimTypes.Role, "reader"),
                new Claim("provider", "entra")
            }, "test"));

            // Act
            decisionLogger.LogDecision(principal, "Reader", true);

            // Assert
            logger.Messages.ShouldNotBeEmpty();
            logger.Messages.ShouldContain(message => message.Contains("user=specific-user-id", StringComparison.OrdinalIgnoreCase));
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
