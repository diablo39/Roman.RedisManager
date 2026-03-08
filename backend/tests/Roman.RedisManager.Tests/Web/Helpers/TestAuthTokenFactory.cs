using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Roman.RedisManager.Tests.Web.Helpers
{
    public static class TestAuthTokenFactory
    {
        public static void ApplyBearer(HttpClient client, params string[] roles)
        {
            ApplyBearer(client, "https://login.microsoftonline.com/common/v2.0", "entra", roles);
        }

        public static void ApplyBearer(HttpClient client, string issuer, string providerKey, params string[] roles)
        {
            var token = CreateToken(issuer, providerKey, roles, null);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public static void ApplyBearer(HttpClient client, string issuer, string providerKey, IDictionary<string, object> additionalClaims, params string[] roles)
        {
            var token = CreateToken(issuer, providerKey, roles, additionalClaims);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private static string CreateToken(string issuer, string providerKey, IEnumerable<string> roles, IDictionary<string, object>? additionalClaims)
        {
            var header = new Dictionary<string, object>
            {
                ["alg"] = "none",
                ["typ"] = "JWT"
            };

            var roleArray = roles.ToArray();
            var payload = new Dictionary<string, object>
            {
                ["sub"] = "test-user",
                ["iss"] = issuer,
                ["provider"] = providerKey,
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
            return $"{encodedHeader}.{encodedPayload}.";
        }

        private static string Base64UrlEncode(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}
