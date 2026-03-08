using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Roman.RedisManager.Web.Authentication
{
    public class BearerHeaderAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public BearerHeaderAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authorizationValues))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var authorization = authorizationValues.ToString();
            if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var token = authorization.Substring("Bearer ".Length).Trim();
            var claims = ParseClaims(token);
            if (claims is null)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid bearer token format."));
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name, ClaimTypes.NameIdentifier, ClaimTypes.Role);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        private static IReadOnlyCollection<Claim>? ParseClaims(string token)
        {
            var segments = token.Split('.');
            if (segments.Length < 2)
            {
                return null;
            }

            var payload = segments[1];
            var jsonBytes = DecodeBase64Url(payload);
            if (jsonBytes is null)
            {
                return null;
            }

            using var document = JsonDocument.Parse(jsonBytes);
            var root = document.RootElement;
            var claims = new List<Claim>();

            if (root.TryGetProperty("sub", out var subElement))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, subElement.GetString() ?? string.Empty));
            }

            if (root.TryGetProperty("iss", out var issuerElement))
            {
                claims.Add(new Claim("iss", issuerElement.GetString() ?? string.Empty));
            }

            if (root.TryGetProperty("provider", out var providerElement))
            {
                claims.Add(new Claim("provider", providerElement.GetString() ?? string.Empty));
            }

            if (root.TryGetProperty("groups", out var groupsElement))
            {
                AppendStringOrArrayClaims(claims, "groups", groupsElement);
            }

            if (root.TryGetProperty("roles", out var rolesElement))
            {
                AppendStringOrArrayClaims(claims, "roles", rolesElement);
            }

            if (root.TryGetProperty("role", out var roleElement))
            {
                if (roleElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in roleElement.EnumerateArray())
                    {
                        var roleValue = item.GetString();
                        if (!string.IsNullOrWhiteSpace(roleValue))
                        {
                            claims.Add(new Claim(ClaimTypes.Role, roleValue));
                        }
                    }
                }
                else if (roleElement.ValueKind == JsonValueKind.String)
                {
                    var roleValue = roleElement.GetString();
                    if (!string.IsNullOrWhiteSpace(roleValue))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, roleValue));
                    }
                }
            }

            return claims;
        }

        private static void AppendStringOrArrayClaims(ICollection<Claim> claims, string claimType, JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    var value = item.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        claims.Add(new Claim(claimType, value));
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.String)
            {
                var value = element.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    claims.Add(new Claim(claimType, value));
                }
            }
        }

        private static byte[]? DecodeBase64Url(string value)
        {
            var padding = 4 - (value.Length % 4);
            if (padding < 4)
            {
                value += new string('=', padding);
            }

            value = value.Replace('-', '+').Replace('_', '/');

            try
            {
                return Convert.FromBase64String(value);
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }
}
