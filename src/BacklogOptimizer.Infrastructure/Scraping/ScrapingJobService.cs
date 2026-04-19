using BacklogOptimizer.Application.Scraping;
using BacklogOptimizer.Core.Common;
using BacklogOptimizer.Core.Entities;
using BacklogOptimizer.Infrastructure.Persistence;

namespace BacklogOptimizer.Infrastructure.Scraping;

internal sealed class ScrapingJobService : IScrapingService
{
    private readonly ApplicationDbContext _dbContext;

    public ScrapingJobService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> EnqueueScrapeAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            return Errors.Validation.UrlRequired;

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            return Errors.Validation.UrlMalformed(url);

        var job = new ScrapingJob(url);
        await _dbContext.ScrapingJobs.AddAsync(job, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
