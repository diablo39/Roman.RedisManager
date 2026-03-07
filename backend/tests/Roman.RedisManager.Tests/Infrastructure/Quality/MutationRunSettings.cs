// -----------------------------------------------------------------------------
// Quality helper – configuration for mutation runs
//
// This simple data holder mirrors options used by the PowerShell scripts. It
// exists in the test project solely to make the CLI argument tests deterministic
// and strongly typed. The scripts are not dependent on this class.
// -----------------------------------------------------------------------------
namespace Roman.RedisManager.Tests.Infrastructure.Quality
{
    public class MutationRunSettings
    {
        public string SolutionPath { get; init; } = "Roman.RedisManager.slnx";

        public string TestProjectPath { get; init; } = "tests/Roman.RedisManager.Tests/Roman.RedisManager.Tests.csproj";

        public string MutableProjectPath { get; init; } = "src/Roman.RedisManager.Application/Roman.RedisManager.Application.csproj";

        public string ConfigFilePath { get; init; } = "tests/Roman.RedisManager.Tests/stryker-config.json";

        public string OutputDirectory { get; init; } = "tests/Roman.RedisManager.Tests/StrykerOutput";

        public bool WithBaseline { get; init; }

        public string? BaselineDirectory { get; init; }

        public IReadOnlyCollection<string> Reporters { get; init; } = new[] { "html", "json" };
    }
}
