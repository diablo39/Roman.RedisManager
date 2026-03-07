// -----------------------------------------------------------------------------
// Quality helper – validates that mutation reports exist
//
// This helper implements the same file-existence checks performed by the
// PowerShell scripts after a run. Keeping it in C# makes the checks testable in
// xUnit, but the scripts do not depend on this type at runtime.
// -----------------------------------------------------------------------------
namespace Roman.RedisManager.Tests.Infrastructure.Quality
{
    public sealed record MutationReportSetValidationResult(
        bool IsValid,
        string? JsonReportPath,
        string? HtmlReportPath,
        string? ComparisonReportPath,
        IReadOnlyCollection<string> Errors);

    public static class MutationReportSetValidator
    {
        public static MutationReportSetValidationResult Validate(string outputDirectory, bool requireComparisonReport)
        {
            var errors = new List<string>();

            if (!Directory.Exists(outputDirectory))
            {
                errors.Add($"Output directory not found: {outputDirectory}");
                return new MutationReportSetValidationResult(false, null, null, null, errors);
            }

            var jsonReportPath = Directory.GetFiles(outputDirectory, "mutation-report.json", SearchOption.AllDirectories).FirstOrDefault();
            var htmlReportPath = Directory.GetFiles(outputDirectory, "mutation-report.html", SearchOption.AllDirectories).FirstOrDefault();
            var comparisonReportPath = Directory.GetFiles(outputDirectory, "mutation-comparison.md", SearchOption.AllDirectories).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(jsonReportPath))
            {
                errors.Add("Mutation JSON report was not found.");
            }

            if (string.IsNullOrWhiteSpace(htmlReportPath))
            {
                errors.Add("Mutation HTML report was not found.");
            }

            if (requireComparisonReport && string.IsNullOrWhiteSpace(comparisonReportPath))
            {
                errors.Add("Mutation comparison report was not found.");
            }

            return new MutationReportSetValidationResult(errors.Count == 0, jsonReportPath, htmlReportPath, comparisonReportPath, errors);
        }
    }
}
