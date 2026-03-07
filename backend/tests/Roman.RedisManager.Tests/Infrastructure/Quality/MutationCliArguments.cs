// -----------------------------------------------------------------------------
// Quality helper – argument builder for mutation CLI
//
// This class implements logic that is also duplicated in the PowerShell
// orchestration scripts. It is defined in the test project so we can verify
// correct argument composition with unit tests. The scripts themselves do not
// reference this type.
// -----------------------------------------------------------------------------
namespace Roman.RedisManager.Tests.Infrastructure.Quality
{
    public static class MutationCliArguments
    {
        public static IReadOnlyCollection<string> Build(MutationRunSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            var reporters = settings.Reporters.Count > 0 ? settings.Reporters : new[] { "html", "json" };
            var args = new List<string>
            {
                "dotnet-stryker",
                "--solution", settings.SolutionPath,
                "--test-project", settings.TestProjectPath,
                "--project", settings.MutableProjectPath,
                "--config-file", settings.ConfigFilePath,
                "--output", settings.OutputDirectory
            };

            foreach (var reporter in reporters)
            {
                args.Add("--reporter");
                args.Add(reporter);
            }

            if (settings.WithBaseline)
            {
                args.AddRange([
                    "--with-baseline"
                ]);
            }

            return args;
        }
    }
}
