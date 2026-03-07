// -----------------------------------------------------------------------------
// Quality governance tests – ensure policy phrases survive regeneration
//
// The scripts and workflow are governed by durable text across multiple files.
// These tests read the same files and verify that the "run -> analyze ->
// improve -> rerun" phrase is present. The production scripts do not call any
// of the types in this folder.
// -----------------------------------------------------------------------------
namespace Roman.RedisManager.Tests.Infrastructure.Quality
{
    public class GovernanceMutationPolicyTests
    {
        [Fact]
        public void GovernanceFiles_MutationLoopPolicyPresent_ContainsRequiredPhrases()
        {

            // Arrange
            var backendRoot = ResolveBackendRoot();
            var files = new[]
            {
                Path.Combine(backendRoot, ".github", "copilot-instructions.md"),
                Path.Combine(backendRoot, ".specify", "memory", "constitution.md"),
                Path.Combine(backendRoot, ".specify", "templates", "spec-template.md"),
                Path.Combine(backendRoot, ".specify", "templates", "plan-template.md"),
                Path.Combine(backendRoot, ".specify", "templates", "tasks-template.md"),
                Path.Combine(backendRoot, ".github", "prompts", "speckit.implement.prompt.md"),
                Path.Combine(backendRoot, ".github", "prompts", "speckit.tasks.prompt.md"),
                Path.Combine(backendRoot, "..", ".specify", "templates", "agent-file-template.md")
            };

            foreach (var file in files)

            {

            // Act

            // Assert
                File.Exists(file).ShouldBeTrue($"Expected governance file not found: {file}");

                var content = File.ReadAllText(file);
                content.ShouldContain("run -> analyze -> improve -> rerun");
                content.ShouldContain("mutation");
            }
        }

        private static string ResolveBackendRoot()
        {
            var currentDirectory = AppContext.BaseDirectory;
            var directoryInfo = new DirectoryInfo(currentDirectory);

            while (directoryInfo is not null)
            {
                if (File.Exists(Path.Combine(directoryInfo.FullName, "Roman.RedisManager.slnx")))
                {
                    return directoryInfo.FullName;
                }

                directoryInfo = directoryInfo.Parent;
            }

            throw new DirectoryNotFoundException("Unable to resolve backend root directory from test execution path.");
        }
    }
}
