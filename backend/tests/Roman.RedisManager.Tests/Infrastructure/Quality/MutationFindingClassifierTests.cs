// -----------------------------------------------------------------------------
// Quality helper tests – mutation finding classifier
//
// Ensures that surviving, no-coverage, and timeout mutants produce appropriate
// findings. The scripts generate similar output but do not reference this
// class; it is purely for verification via unit tests.
// -----------------------------------------------------------------------------
namespace Roman.RedisManager.Tests.Infrastructure.Quality
{
    public class MutationFindingClassifierTests
    {
        [Fact]
        public void Classify_SurvivedAndNoCoverageMutants_ReturnsActionableFindings()
        {
            var classifier = new MutationFindingClassifier();
            var mutants = new[]
            {
                new MutationMutant("1", "Survived", "src/Foo.cs", 12),
                new MutationMutant("2", "NoCoverage", "src/Foo.cs", 25)
            };

            var findings = classifier.Classify(mutants);

            findings.Count.ShouldBe(2);
            findings.ShouldContain(f => f.Category == MutationFindingCategory.SurvivedMutant && f.Severity == MutationFindingSeverity.High);
            findings.ShouldContain(f => f.Category == MutationFindingCategory.NoCoverage && f.Severity == MutationFindingSeverity.Medium);
        }

        [Fact]
        public void Classify_TimeoutMutant_ReturnsFlakyOutcomeFinding()
        {
            var classifier = new MutationFindingClassifier();
            var mutants = new[]
            {
                new MutationMutant("3", "Timeout", "src/Bar.cs", 4)
            };

            var findings = classifier.Classify(mutants);

            findings.Count.ShouldBe(1);
            findings.Single().Category.ShouldBe(MutationFindingCategory.FlakyOutcome);
            findings.Single().Recommendation.ShouldNotBeNullOrWhiteSpace();
        }
    }
}
