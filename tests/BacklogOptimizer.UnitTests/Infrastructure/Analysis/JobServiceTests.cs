using BacklogOptimizer.Core.Entities;
using BacklogOptimizer.Infrastructure.Analysis;
using BacklogOptimizer.Infrastructure.Persistence;
using BacklogOptimizer.UnitTests.Helpers;

using Microsoft.EntityFrameworkCore;

namespace BacklogOptimizer.UnitTests.Infrastructure.Analysis;

public class JobServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly FeatureSuggestionJobService _featureService;
    private readonly ReprioritizationJobService _reprioritizationService;

    public JobServiceTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _featureService = new FeatureSuggestionJobService(_dbContext);
        _reprioritizationService = new ReprioritizationJobService(_dbContext);
    }

    public void Dispose() => _dbContext.Dispose();

    [Fact]
    public async Task FeatureSuggestionJobService_EnqueueAnalysisAsync_ReturnsSuccess()
    {
        var result = await _featureService.EnqueueAnalysisAsync();

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task FeatureSuggestionJobService_EnqueueAnalysisAsync_CreatesJobInDb()
    {
        await _featureService.EnqueueAnalysisAsync();

        var job = await _dbContext.FeatureSuggestionJobs.SingleAsync();
        Assert.Equal(JobStatus.Pending, job.Status);
    }

    [Fact]
    public async Task FeatureSuggestionJobService_MultipleEnqueues_CreatesMultipleJobs()
    {
        await _featureService.EnqueueAnalysisAsync();
        await _featureService.EnqueueAnalysisAsync();

        Assert.Equal(2, await _dbContext.FeatureSuggestionJobs.CountAsync());
    }

    [Fact]
    public async Task ReprioritizationJobService_EnqueueAnalysisAsync_ReturnsSuccess()
    {
        var result = await _reprioritizationService.EnqueueAnalysisAsync();

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ReprioritizationJobService_EnqueueAnalysisAsync_CreatesJobInDb()
    {
        await _reprioritizationService.EnqueueAnalysisAsync();

        var job = await _dbContext.ReprioritizationJobs.SingleAsync();
        Assert.Equal(JobStatus.Pending, job.Status);
    }
}
