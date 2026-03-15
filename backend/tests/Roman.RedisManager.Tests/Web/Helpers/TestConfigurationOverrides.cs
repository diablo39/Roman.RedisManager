namespace Roman.RedisManager.Tests.Web.Helpers
{
    public sealed record RoleClaimMappingOverride(
        string RoleName,
        string ProviderKey,
        string ClaimKey,
        IReadOnlyCollection<string> AllowedValues,
        string MatchMode = "Any");

    public static class TestConfigurationOverrides
    {
        public static Dictionary<string, string?> ForRoleClaimMappings(params RoleClaimMappingOverride[] mappings)
        {
            var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            for (var mappingIndex = 0; mappingIndex < mappings.Length; mappingIndex++)
            {
                var mapping = mappings[mappingIndex];
                var mappingPath = $"Security:Authorization:RoleClaimMappings:{mappingIndex}";

                values[$"{mappingPath}:RoleName"] = mapping.RoleName;
                values[$"{mappingPath}:ProviderKey"] = mapping.ProviderKey;
                values[$"{mappingPath}:ClaimKey"] = mapping.ClaimKey;
                values[$"{mappingPath}:MatchMode"] = mapping.MatchMode;

                var valueIndex = 0;
                foreach (var allowedValue in mapping.AllowedValues)
                {
                    values[$"{mappingPath}:AllowedValues:{valueIndex}"] = allowedValue;
                    valueIndex++;
                }
            }

            return values;
        }
    }
}
