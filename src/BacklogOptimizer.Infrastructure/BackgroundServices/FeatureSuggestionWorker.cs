using BacklogOptimizer.Application.Settings;
using BacklogOptimizer.Core.Entities;
using BacklogOptimizer.Infrastructure.Analysis;
using BacklogOptimizer.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BacklogOptimizer.Infrastructure.BackgroundServices;

internal sealed class FeatureSuggestionWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FeatureSuggestionWorker> _logger;
    private readonly TimeSpan _pollingInterval;

    public FeatureSuggestionWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<FeatureSuggestionWorker> logger,
        IOptions<AnalysisWorkerSettings> workerSettings)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _pollingInterval = TimeSpan.FromSeconds(workerSettings.Value.PollingIntervalSeconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FeatureSuggestionWorker started");

        await ResetStuckJobsAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessNextJobAsync(stoppingToken);

            try
            {
                await Task.Delay(_pollingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("FeatureSuggestionWorker stopped");
    }

    private async Task ResetStuckJobsAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var stuckJobs = await dbContext.FeatureSuggestionJobs
            .Where(j => j.Status == JobStatus.Running)
            .ToListAsync(cancellationToken);

        if (stuckJobs.Count == 0)
            return;

        foreach (var job in stuckJobs)
            job.MarkFailed("Interrupted by application restart");

        await dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogWarning("Reset {Count} stuck feature suggestion jobs", stuckJobs.Count);
    }

    private async Task ProcessNextJobAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var job = await dbContext.FeatureSuggestionJobs
            .Where(j => j.Status == JobStatus.Pending)
            .OrderBy(j => j.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (job is null)
            return;

        job.MarkRunning();
        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Starting feature suggestion job {JobId}", job.Id);

        try
        {
            var analyzer = scope.ServiceProvider.GetRequiredService<FeatureSuggestionAnalyzer>();
            var result = await analyzer.AnalyzeAsync(cancellationToken);

            if (result.IsSuccess)
            {
                job.MarkCompleted();
                _logger.LogInformation(
                    "Completed feature suggestion job {JobId}: {Gaps} gap pages, {Count} suggestions",
                    job.Id, result.Value!.GapPagesFound, result.Value.SuggestionsGenerated);
            }
            else
            {
                job.MarkFailed(result.Error!.Description);
                _logger.LogError("Failed feature suggestion job {JobId}: {Error}", job.Id, result.Error.Description);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed feature suggestion job {JobId}", job.Id);
            job.MarkFailed(ex.Message);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
