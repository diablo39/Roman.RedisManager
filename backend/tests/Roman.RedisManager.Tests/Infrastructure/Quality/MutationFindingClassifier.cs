// -----------------------------------------------------------------------------
// Quality helper – classifies mutation mutants into findings
//
// Converts individual mutant records into high/medium/low severity findings
// used by the analysis report. This logic parallels, but is separate from, the
// shell script `analyze-surviving-mutants.ps1`; the C# version exists only to
// make the behaviour unit-testable.
// -----------------------------------------------------------------------------
namespace Roman.RedisManager.Tests.Infrastructure.Quality
{
    public sealed record MutationMutant(string Id, string Status, string FilePath, int Line);

    public sealed class MutationFindingClassifier
    {
        public IReadOnlyCollection<MutationFinding> Classify(IReadOnlyCollection<MutationMutant> mutants)
        {
            ArgumentNullException.ThrowIfNull(mutants);

            var findings = new List<MutationFinding>();
            foreach (var mutant in mutants)
            {
                if (string.Equals(mutant.Status, "Survived", StringComparison.OrdinalIgnoreCase))
                {
                    findings.Add(new MutationFinding(
                        $"mutant-{mutant.Id}",
                        MutationFindingCategory.SurvivedMutant,
                        MutationFindingSeverity.High,
                        $"{mutant.FilePath}:{mutant.Line}",
                        "Strengthen assertions for this behavior and rerun mutation analysis."));
                }
                else if (string.Equals(mutant.Status, "NoCoverage", StringComparison.OrdinalIgnoreCase))
                {
                    findings.Add(new MutationFinding(
                        $"mutant-{mutant.Id}",
                        MutationFindingCategory.NoCoverage,
                        MutationFindingSeverity.Medium,
                        $"{mutant.FilePath}:{mutant.Line}",
                        "Add targeted tests for this behavior and verify the mutant is killed."));
                }
                else if (string.Equals(mutant.Status, "Timeout", StringComparison.OrdinalIgnoreCase))
                {
                    findings.Add(new MutationFinding(
                        $"mutant-{mutant.Id}",
                        MutationFindingCategory.FlakyOutcome,
                        MutationFindingSeverity.Low,
                        $"{mutant.FilePath}:{mutant.Line}",
                        "Re-run the mutation test and quarantine flaky outcomes before triage."));
                }
            }

            return findings;
        }
    }
}
