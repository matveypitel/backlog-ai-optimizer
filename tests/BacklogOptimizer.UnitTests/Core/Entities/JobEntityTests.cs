using BacklogOptimizer.Core.Entities;

namespace BacklogOptimizer.UnitTests.Core.Entities;

public class JobEntityTests
{
    [Fact]
    public void FeatureSuggestionJob_DefaultIsPending()
    {
        var job = new FeatureSuggestionJob();

        Assert.Equal(JobStatus.Pending, job.Status);
        Assert.Null(job.ErrorMessage);
    }

    [Fact]
    public void FeatureSuggestionJob_MarkRunning_SetsRunning()
    {
        var job = new FeatureSuggestionJob();

        job.MarkRunning();

        Assert.Equal(JobStatus.Running, job.Status);
    }

    [Fact]
    public void FeatureSuggestionJob_MarkCompleted_SetsCompleted()
    {
        var job = new FeatureSuggestionJob();
        job.MarkRunning();

        job.MarkCompleted();

        Assert.Equal(JobStatus.Completed, job.Status);
    }

    [Fact]
    public void FeatureSuggestionJob_MarkFailed_SetsFailedAndMessage()
    {
        var job = new FeatureSuggestionJob();

        job.MarkFailed("analysis error");

        Assert.Equal(JobStatus.Failed, job.Status);
        Assert.Equal("analysis error", job.ErrorMessage);
    }

    [Fact]
    public void ReprioritizationJob_DefaultIsPending()
    {
        var job = new ReprioritizationJob();

        Assert.Equal(JobStatus.Pending, job.Status);
        Assert.Null(job.ErrorMessage);
    }

    [Fact]
    public void ReprioritizationJob_MarkRunning_SetsRunning()
    {
        var job = new ReprioritizationJob();

        job.MarkRunning();

        Assert.Equal(JobStatus.Running, job.Status);
    }

    [Fact]
    public void ReprioritizationJob_MarkCompleted_SetsCompleted()
    {
        var job = new ReprioritizationJob();
        job.MarkRunning();

        job.MarkCompleted();

        Assert.Equal(JobStatus.Completed, job.Status);
    }

    [Fact]
    public void ReprioritizationJob_MarkFailed_SetsFailedAndMessage()
    {
        var job = new ReprioritizationJob();

        job.MarkFailed("reprioritization failed");

        Assert.Equal(JobStatus.Failed, job.Status);
        Assert.Equal("reprioritization failed", job.ErrorMessage);
    }
}
