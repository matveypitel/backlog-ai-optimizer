using BacklogOptimizer.Application.Scraping;
using BacklogOptimizer.Application.Settings;
using BacklogOptimizer.Core.Entities;
using BacklogOptimizer.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace BacklogOptimizer.Infrastructure.BackgroundServices;

internal sealed class ScrapingWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScrapingWorker> _logger;
    private readonly TimeSpan _pollingInterval;
    private readonly ScrapingSettings _scrapingSettings;

    public ScrapingWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<ScrapingWorker> logger,
        IOptions<ScrapingWorkerSettings> workerSettings,
        IOptions<ScrapingSettings> scrapingSettings)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _pollingInterval = TimeSpan.FromSeconds(workerSettings.Value.PollingIntervalSeconds);
        _scrapingSettings = scrapingSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ScrapingWorker started");

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

        _logger.LogInformation("ScrapingWorker stopped");
    }

    private async Task ResetStuckJobsAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var stuckJobs = await dbContext.ScrapingJobs
            .Where(j => j.Status == ScrapingJobStatus.Running)
            .ToListAsync(cancellationToken);

        if (stuckJobs.Count == 0)
            return;

        foreach (var job in stuckJobs)
            job.MarkFailed("Interrupted by application restart");

        await dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogWarning("Reset {Count} stuck scraping jobs", stuckJobs.Count);
    }

    private async Task ProcessNextJobAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var job = await dbContext.ScrapingJobs
            .Where(j => j.Status == ScrapingJobStatus.Pending)
            .OrderBy(j => j.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (job is null)
            return;

        job.MarkRunning();
        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Starting scraping job {JobId} for {Url}", job.Id, job.Url);

        try
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = _scrapingSettings.Headless
            });

            var page = await browser.NewPageAsync();
            await page.GotoAsync(job.Url, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = _scrapingSettings.Timeout
            });

            var title = await page.TitleAsync();
            var htmlContent = await page.ContentAsync();
            var extractedText = await page.InnerTextAsync("body");

            var existing = await dbContext.ScrapedPages
                .FirstOrDefaultAsync(p => p.Url == job.Url, cancellationToken);

            if (existing is not null)
                existing.Update(htmlContent, title, extractedText);
            else
                await dbContext.ScrapedPages.AddAsync(new ScrapedPage(job.Url, htmlContent, title, extractedText), cancellationToken);

            job.MarkCompleted();
            _logger.LogInformation("Completed scraping job {JobId} for {Url}", job.Id, job.Url);
        }
        catch (Exception ex)
        {
            job.IncrementAttemptCount();

            if (job.AttemptCount < _scrapingSettings.MaxRetryAttempts)
            {
                _logger.LogInformation("Retrying scraping job {JobId} for {Url} (attempt {Attempt})", job.Id, job.Url, job.AttemptCount);

                job.MarkForRetry();

                var delay = TimeSpan.FromMilliseconds(_scrapingSettings.MaxRetryDelay * Math.Pow(2, job.AttemptCount));
                await Task.Delay(delay, cancellationToken);
            }
            else
            {
                _logger.LogError(ex, "Failed scraping job {JobId} for {Url}", job.Id, job.Url);
                job.MarkFailed(ex.Message);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
