using BacklogOptimizer.Core.Common;

namespace BacklogOptimizer.Core.Entities;

public class ScrapingJob : BaseEntity
{
    public string Url { get; private set; }
    public ScrapingJobStatus Status { get; private set; } = ScrapingJobStatus.Pending;
    public string? ErrorMessage { get; private set; }
    public int AttemptCount { get; private set; }

    public ScrapingJob(string url)
    {
        Url = url;
    }

    public void MarkRunning()
    {
        Status = ScrapingJobStatus.Running;
        AttemptCount++;
    }

    public void MarkCompleted()
    {
        Status = ScrapingJobStatus.Completed;
    }

    public void MarkFailed(string errorMessage)
    {
        Status = ScrapingJobStatus.Failed;
        ErrorMessage = errorMessage;
    }
}
