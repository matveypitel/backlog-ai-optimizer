using BacklogOptimizer.Core.Entities;

namespace BacklogOptimizer.UnitTests.Core.Entities;

public class ScrapingJobTests
{
    [Fact]
    public void Constructor_SetsUrlAndDefaultStatus()
    {
        var job = new ScrapingJob("https://example.com");

        Assert.Equal("https://example.com", job.Url);
        Assert.Equal(ScrapingJobStatus.Pending, job.Status);
        Assert.Equal(0, job.AttemptCount);
        Assert.Null(job.ErrorMessage);
        Assert.Null(job.SourceId);
    }

    [Fact]
    public void Constructor_WithSourceId_SetsSourceId()
    {
        var sourceId = Guid.NewGuid();
        var job = new ScrapingJob("https://example.com", sourceId);

        Assert.Equal(sourceId, job.SourceId);
    }

    [Fact]
    public void MarkRunning_SetsStatusAndIncrementsAttemptCount()
    {
        var job = new ScrapingJob("https://example.com");

        job.MarkRunning();

        Assert.Equal(ScrapingJobStatus.Running, job.Status);
        Assert.Equal(1, job.AttemptCount);
    }

    [Fact]
    public void MarkRunning_CalledTwice_AttemptCountIsTwo()
    {
        var job = new ScrapingJob("https://example.com");

        job.MarkRunning();
        job.MarkRunning();

        Assert.Equal(2, job.AttemptCount);
    }

    [Fact]
    public void MarkCompleted_SetsCompletedStatus()
    {
        var job = new ScrapingJob("https://example.com");
        job.MarkRunning();

        job.MarkCompleted();

        Assert.Equal(ScrapingJobStatus.Completed, job.Status);
    }

    [Fact]
    public void MarkFailed_SetsStatusAndErrorMessage()
    {
        var job = new ScrapingJob("https://example.com");

        job.MarkFailed("timeout error");

        Assert.Equal(ScrapingJobStatus.Failed, job.Status);
        Assert.Equal("timeout error", job.ErrorMessage);
    }

    [Fact]
    public void MarkForRetry_SetsPendingStatus()
    {
        var job = new ScrapingJob("https://example.com");
        job.MarkFailed("error");

        job.MarkForRetry();

        Assert.Equal(ScrapingJobStatus.Pending, job.Status);
    }

    [Fact]
    public void IncrementAttemptCount_IncrementsWithoutChangingStatus()
    {
        var job = new ScrapingJob("https://example.com");

        job.IncrementAttemptCount();
        job.IncrementAttemptCount();

        Assert.Equal(2, job.AttemptCount);
        Assert.Equal(ScrapingJobStatus.Pending, job.Status);
    }
}
