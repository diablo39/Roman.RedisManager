// -----------------------------------------------------------------------------
// Quality helper tests – report validation
//
// Verifies that the mutation run produces the expected JSON/HTML/comparison
// reports. Equivalent validation code appears in the `run-mutation-tests.ps1`
// script; this class is included only for the sake of unit testing.
// -----------------------------------------------------------------------------
namespace Roman.RedisManager.Tests.Infrastructure.Quality
{
    public class MutationReportSetValidatorTests
    {
        [Fact]
        public void Validate_RequiredReportsExist_ReturnsValid()
        {

            // Arrange
            var outputDirectory = CreateTempOutputDirectory();
            Directory.CreateDirectory(Path.Combine(outputDirectory, "reports"));
            File.WriteAllText(Path.Combine(outputDirectory, "reports", "mutation-report.json"), "{}");
            File.WriteAllText(Path.Combine(outputDirectory, "reports", "mutation-report.html"), "<html></html>");
            File.WriteAllText(Path.Combine(outputDirectory, "mutation-comparison.md"), "# comparison");

            // Act
            var result = MutationReportSetValidator.Validate(outputDirectory, requireComparisonReport: true);

            // Assert
            result.IsValid.ShouldBeTrue();
            result.Errors.ShouldBeEmpty();
            result.JsonReportPath.ShouldNotBeNullOrWhiteSpace();
            result.HtmlReportPath.ShouldNotBeNullOrWhiteSpace();
            result.ComparisonReportPath.ShouldNotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Validate_MissingComparisonWhenRequired_ReturnsInvalid()
        {

            // Arrange
            var outputDirectory = CreateTempOutputDirectory();
            File.WriteAllText(Path.Combine(outputDirectory, "mutation-report.json"), "{}");
            File.WriteAllText(Path.Combine(outputDirectory, "mutation-report.html"), "<html></html>");

            // Act
            var result = MutationReportSetValidator.Validate(outputDirectory, requireComparisonReport: true);

            // Assert
            result.IsValid.ShouldBeFalse();
            result.Errors.ShouldContain("Mutation comparison report was not found.");
        }

        private static string CreateTempOutputDirectory()
        {
            var directory = Path.Combine(Path.GetTempPath(), "mutation-validator-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            return directory;
        }
    }
}
