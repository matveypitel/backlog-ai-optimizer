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

internal sealed class ReprioritizationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReprioritizationWorker> _logger;
    private readonly TimeSpan _pollingInterval;

    public ReprioritizationWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<ReprioritizationWorker> logger,
        IOptions<AnalysisWorkerSettings> workerSettings)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _pollingInterval = TimeSpan.FromSeconds(workerSettings.Value.PollingIntervalSeconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReprioritizationWorker started");

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

        _logger.LogInformation("ReprioritizationWorker stopped");
    }

    private async Task ResetStuckJobsAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var stuckJobs = await dbContext.ReprioritizationJobs
            .Where(j => j.Status == JobStatus.Running)
            .ToListAsync(cancellationToken);

        if (stuckJobs.Count == 0)
            return;

        foreach (var job in stuckJobs)
            job.MarkFailed("Interrupted by application restart");

        await dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogWarning("Reset {Count} stuck reprioritization jobs", stuckJobs.Count);
    }

    private async Task ProcessNextJobAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var job = await dbContext.ReprioritizationJobs
            .Where(j => j.Status == JobStatus.Pending)
            .OrderBy(j => j.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (job is null)
            return;

        job.MarkRunning();
        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Starting reprioritization job {JobId}", job.Id);

        try
        {
            var analyzer = scope.ServiceProvider.GetRequiredService<ReprioritizationAnalyzer>();
            var result = await analyzer.AnalyzeAsync(cancellationToken);

            if (result.IsSuccess)
            {
                job.MarkCompleted();
                _logger.LogInformation(
                    "Completed reprioritization job {JobId}: {Analyzed} analyzed, {Skipped} skipped, {Errors} errors",
                    job.Id, result.Value!.AnalyzedCount, result.Value.SkippedCount, result.Value.Errors.Count);
            }
            else
            {
                job.MarkFailed(result.Error!.Description);
                _logger.LogError("Failed reprioritization job {JobId}: {Error}", job.Id, result.Error.Description);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed reprioritization job {JobId}", job.Id);
            job.MarkFailed(ex.Message);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
