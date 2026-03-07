// -----------------------------------------------------------------------------
// Quality helper – data model for mutation findings
//
// Represents the actionable items generated from mutation-report analysis. The
// class is used by the classifier and the analysis script, but the script uses a
// PoSH-specific representation; this type exists to make testing easier in C#.
// -----------------------------------------------------------------------------
namespace Roman.RedisManager.Tests.Infrastructure.Quality
{
    public enum MutationFindingCategory
    {
        SurvivedMutant,
        NoCoverage,
        PotentialDuplicateTest,
        FlakyOutcome,
        NonActionable
    }

    public enum MutationFindingSeverity
    {
        High,
        Medium,
        Low
    }

    public sealed record MutationFinding(
        string FindingId,
        MutationFindingCategory Category,
        MutationFindingSeverity Severity,
        string Location,
        string Recommendation);
}
