// -----------------------------------------------------------------------------
// Quality helper – heuristics for duplicate/low-differentiation tests
//
// Contains logic used to flag tests that overlap excessively in mutant outcomes
// without unique assertion intent. This logic is exercised by the corresponding
// unit tests; the PowerShell scripts duplicate the same behavior independently.
// -----------------------------------------------------------------------------
namespace Roman.RedisManager.Tests.Infrastructure.Quality
{
    public sealed record TestMutationFootprint(
        string TestName,
        IReadOnlyCollection<string> MutantIds,
        IReadOnlyCollection<string> AssertionCategories,
        string BehaviorArea);

    public sealed class DuplicateTestHeuristicAnalyzer
    {
        public IReadOnlyCollection<MutationFinding> Analyze(IReadOnlyCollection<TestMutationFootprint> footprints)
        {
            ArgumentNullException.ThrowIfNull(footprints);

            var findings = new List<MutationFinding>();
            for (int i = 0; i < footprints.Count; i++)
            {
                for (int j = i + 1; j < footprints.Count; j++)
                {
                    var left = footprints.ElementAt(i);
                    var right = footprints.ElementAt(j);
                    if (!string.Equals(left.BehaviorArea, right.BehaviorArea, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var overlap = ComputeOverlap(left.MutantIds, right.MutantIds);
                    var hasUniqueAssertionIntent = left.AssertionCategories.Except(right.AssertionCategories, StringComparer.OrdinalIgnoreCase).Any()
                        || right.AssertionCategories.Except(left.AssertionCategories, StringComparer.OrdinalIgnoreCase).Any();

                    if (overlap >= 0.8m && !hasUniqueAssertionIntent)
                    {
                        findings.Add(new MutationFinding(
                            $"dup-{left.TestName}-{right.TestName}",
                            MutationFindingCategory.PotentialDuplicateTest,
                            MutationFindingSeverity.Medium,
                            left.BehaviorArea,
                            "Consolidate or differentiate these tests by adding unique assertion intent."));
                    }
                }
            }

            return findings;
        }

        private static decimal ComputeOverlap(IReadOnlyCollection<string> leftMutants, IReadOnlyCollection<string> rightMutants)
        {
            if (leftMutants.Count == 0 || rightMutants.Count == 0)
            {
                return 0;
            }

            var left = leftMutants.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var right = rightMutants.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var intersection = left.Intersect(right, StringComparer.OrdinalIgnoreCase).Count();
            var maxSetCount = Math.Max(left.Count, right.Count);

            return maxSetCount == 0 ? 0 : intersection / (decimal)maxSetCount;
        }
    }
}
