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
        [Theory]
        [InlineData(".", "AGENTS.md")]
        [InlineData(".specify", "memory", "constitution.md")]
        [InlineData(".specify", "templates", "spec-template.md")]
        [InlineData(".specify", "templates", "plan-template.md")]
        [InlineData(".specify", "templates", "tasks-template.md")]
        [InlineData(".github", "prompts", "speckit.implement.prompt.md")]
        [InlineData(".github", "prompts", "speckit.tasks.prompt.md")]
        [InlineData("..", ".specify", "templates", "agent-file-template.md")]
        public void GovernanceFiles_MutationLoopPolicyPresent_ContainsRequiredPhrases(params string[] pathSegments)
        {
            // Arrange
            var backendRoot = ResolveBackendRoot();
            var file = Path.Combine([backendRoot, .. pathSegments]);

            // Act
            File.Exists(file).ShouldBeTrue($"Expected governance file not found: {file}");
            var content = File.ReadAllText(file);

            // Assert
            content.ShouldContain("run -> analyze -> improve -> rerun");
            content.ShouldContain("mutation");
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
