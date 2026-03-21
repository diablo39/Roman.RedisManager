using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Roman.RedisManager.Tests.Web.Helpers
{
    public static class TestAuthTokenFactory
    {
        private const string _signingKey = "development-only-signing-key-change-me-1234567890";

        public static void ApplyBearerToken(HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public static void ApplyBearer(HttpClient client, params string[] roles)
        {
            ApplyBearer(client, "https://login.microsoftonline.com/common/v2.0", "entra", roles);
        }

        public static void ApplyBearer(HttpClient client, string issuer, string providerKey, params string[] roles)
        {
            if (!issuer.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                var normalizedRoles = new List<string> { issuer, providerKey };
                normalizedRoles.AddRange(roles);
                ApplyBearer(client, normalizedRoles.ToArray());
                return;
            }

            var token = CreateToken(issuer, providerKey, roles, null);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public static void ApplyBearer(HttpClient client, string issuer, string providerKey, IDictionary<string, object> additionalClaims, params string[] roles)
        {
            var token = CreateToken(issuer, providerKey, roles, additionalClaims);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public static string CreateBearerToken(
            string issuer,
            string providerKey,
            IEnumerable<string> roles,
            IDictionary<string, object>? additionalClaims = null,
            string? signingKey = null)
        {
            return CreateToken(issuer, providerKey, roles, additionalClaims, signingKey);
        }

        private static string CreateToken(
            string issuer,
            string providerKey,
            IEnumerable<string> roles,
            IDictionary<string, object>? additionalClaims,
            string? signingKey = null)
        {
            var header = new Dictionary<string, object>
            {
                ["alg"] = "HS256",
                ["typ"] = "JWT"
            };

            var roleArray = roles.ToArray();
            var payload = new Dictionary<string, object>
            {
                ["sub"] = "test-user",
                ["iss"] = issuer,
                ["provider"] = providerKey,
                ["aud"] = ResolveAudience(providerKey),
                ["exp"] = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()
            };

            if (roleArray.Length > 0)
            {
                payload["role"] = roleArray.Length == 1 ? roleArray[0] : roleArray;
            }

            if (additionalClaims is not null)
            {
                foreach (var claim in additionalClaims)
                {
                    payload[claim.Key] = claim.Value;
                }
            }

            var encodedHeader = Base64UrlEncode(JsonSerializer.Serialize(header));
            var encodedPayload = Base64UrlEncode(JsonSerializer.Serialize(payload));
            var signature = ComputeHmacSha256($"{encodedHeader}.{encodedPayload}", signingKey ?? _signingKey);
            return $"{encodedHeader}.{encodedPayload}.{signature}";
        }

        private static string Base64UrlEncode(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static string ComputeHmacSha256(string value, string signingKey)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(signingKey));
            var signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(value));
            return Convert.ToBase64String(signature)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static string ResolveAudience(string providerKey)
        {
            if (string.Equals(providerKey, "entra", StringComparison.OrdinalIgnoreCase))
            {
                return "dbdde03c-b139-47f1-b076-63b2bf78cabe";
            }

            if (string.Equals(providerKey, "google", StringComparison.OrdinalIgnoreCase))
            {
                return "dev-google-client-id";
            }

            if (string.Equals(providerKey, "generic", StringComparison.OrdinalIgnoreCase))
            {
                return "dev-generic-client-id";
            }

            return providerKey;
        }
    }
}
