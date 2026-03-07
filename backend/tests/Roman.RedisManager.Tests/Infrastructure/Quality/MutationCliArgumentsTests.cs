// -----------------------------------------------------------------------------
// Quality helper tests – see Quality/README.md for context.
//
// This file contains unit tests for helper logic that mirrors the behaviour of
// the mutation-testing PowerShell scripts under
// `backend/.specify/scripts/powershell`. The scripts themselves do not load or
// reference any of the types defined in this directory; they exist solely so
// that we can exercise the same algorithms with `dotnet test`. When the
// workflow implemented in the scripts changes, update these helpers and tests
// accordingly to keep them in sync.
// -----------------------------------------------------------------------------
namespace Roman.RedisManager.Tests.Infrastructure.Quality
{
    public class MutationCliArgumentsTests
    {
        [Fact]
        public void Build_DefaultSettings_ContainsRequiredStrykerArguments()
        {
            var settings = new MutationRunSettings();

            var args = MutationCliArguments.Build(settings);

            args.ShouldContain("dotnet-stryker");
            args.ShouldContain("--solution");
            args.ShouldContain(settings.SolutionPath);
            args.ShouldContain("--test-project");
            args.ShouldContain(settings.TestProjectPath);
            args.ShouldContain("--project");
            args.ShouldContain(settings.MutableProjectPath);
            args.ShouldContain("--config-file");
            args.ShouldContain(settings.ConfigFilePath);
            args.ShouldContain("--reporter");
            args.ShouldContain("html");
            args.ShouldContain("json");
        }

        [Fact]
        public void Build_WithBaselineEnabled_IncludesBaselineArguments()
        {
            var settings = new MutationRunSettings
            {
                WithBaseline = true,
                BaselineDirectory = "tests/Roman.RedisManager.Tests/StrykerOutput/baseline"
            };

            var args = MutationCliArguments.Build(settings);

            args.ShouldContain("--with-baseline");
        }
    }
}
