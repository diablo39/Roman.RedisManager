// -----------------------------------------------------------------------------
// Quality helper tests – assertion improvement cycle state machine
//
// Exercises the in-memory model for tracking the run→rerun→complete workflow.
// The scripts do not reference this class; it merely documents expected
// transitions and helps keep the workflow logic clear.
// -----------------------------------------------------------------------------
namespace Roman.RedisManager.Tests.Infrastructure.Quality
{
    public enum AssertionImprovementCycleStatus
    {
        NotStarted,
        Running,
        Completed
    }

    public sealed class AssertionImprovementCycleState
    {
        public AssertionImprovementCycleStatus Status { get; private set; } = AssertionImprovementCycleStatus.NotStarted;

        public string? RunIdBefore { get; private set; }

        public string? RunIdAfter { get; private set; }

        public string? CompletionReason { get; private set; }

        public void Start(string runIdBefore)
        {
            if (Status != AssertionImprovementCycleStatus.NotStarted)
            {
                throw new InvalidOperationException("Cycle already started.");
            }

            RunIdBefore = runIdBefore;
            Status = AssertionImprovementCycleStatus.Running;
        }

        public void RecordRerun(string runIdAfter)
        {
            if (Status != AssertionImprovementCycleStatus.Running)
            {
                throw new InvalidOperationException("Cycle must be running.");
            }

            RunIdAfter = runIdAfter;
        }

        public void Complete(string reason)
        {
            if (Status != AssertionImprovementCycleStatus.Running)
            {
                throw new InvalidOperationException("Cycle must be running before completion.");
            }

            CompletionReason = reason;
            Status = AssertionImprovementCycleStatus.Completed;
        }
    }

    public class AssertionImprovementCycleTests
    {
        [Fact]
        public void Start_ThenComplete_TransitionsToCompletedState()
        {

            // Arrange
            var cycle = new AssertionImprovementCycleState();

            cycle.Start("run-1");
            cycle.RecordRerun("run-2");
            cycle.Complete("ThresholdMet");

            // Act

            // Assert
            cycle.Status.ShouldBe(AssertionImprovementCycleStatus.Completed);
            cycle.RunIdBefore.ShouldBe("run-1");
            cycle.RunIdAfter.ShouldBe("run-2");
            cycle.CompletionReason.ShouldBe("ThresholdMet");
        }

        [Fact]
        public void Complete_BeforeStart_ThrowsInvalidOperationException()
        {

            // Arrange
            var cycle = new AssertionImprovementCycleState();

            // Act

            // Assert
            Should.Throw<InvalidOperationException>(() => cycle.Complete("ThresholdMet"));
        }
    }
}
