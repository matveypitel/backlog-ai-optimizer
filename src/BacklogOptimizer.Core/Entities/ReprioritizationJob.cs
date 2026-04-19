using BacklogOptimizer.Core.Common;

namespace BacklogOptimizer.Core.Entities;

public class ReprioritizationJob : BaseEntity
{
    public JobStatus Status { get; private set; } = JobStatus.Pending;
    public string? ErrorMessage { get; private set; }

    public void MarkRunning() => Status = JobStatus.Running;

    public void MarkCompleted() => Status = JobStatus.Completed;

    public void MarkFailed(string errorMessage)
    {
        Status = JobStatus.Failed;
        ErrorMessage = errorMessage;
    }
}
