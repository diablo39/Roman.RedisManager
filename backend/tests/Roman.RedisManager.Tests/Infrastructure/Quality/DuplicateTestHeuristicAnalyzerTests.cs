// -----------------------------------------------------------------------------
// Quality helper tests – duplicate test detection
//
// Tests for the analyzer that identifies potential duplicate/low-differentiation
// tests based on mutation outcomes. The production scripts perform similar
// analysis, but these classes are only referenced by unit tests.
// -----------------------------------------------------------------------------
namespace Roman.RedisManager.Tests.Infrastructure.Quality
{
    public class DuplicateTestHeuristicAnalyzerTests
    {
        [Fact]
        public void Analyze_HighOverlapWithoutUniqueAssertionIntent_ReturnsPotentialDuplicate()
        {

            // Arrange
            var analyzer = new DuplicateTestHeuristicAnalyzer();
            var footprints = new[]
            {
                new TestMutationFootprint("A", new[] { "m1", "m2", "m3", "m4", "m5" }, new[] { "equality" }, "value-read"),
                new TestMutationFootprint("B", new[] { "m1", "m2", "m3", "m4", "m6" }, new[] { "equality" }, "value-read")
            };

            var findings = analyzer.Analyze(footprints);

            // Act

            // Assert
            findings.Count.ShouldBe(1);
            findings.Single().Category.ShouldBe(MutationFindingCategory.PotentialDuplicateTest);
        }

        [Fact]
        public void Analyze_UniqueAssertionIntentPresent_ReturnsNoDuplicateFinding()
        {

            // Arrange
            var analyzer = new DuplicateTestHeuristicAnalyzer();
            var footprints = new[]
            {
                new TestMutationFootprint("A", new[] { "m1", "m2", "m3", "m4", "m5" }, new[] { "equality" }, "value-read"),
                new TestMutationFootprint("B", new[] { "m1", "m2", "m3", "m4", "m5" }, new[] { "range" }, "value-read")
            };

            var findings = analyzer.Analyze(footprints);

            // Act

            // Assert
            findings.ShouldBeEmpty();
        }
    }
}
