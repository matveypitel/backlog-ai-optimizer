using BacklogOptimizer.Application.Scraping;
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

    public async Task EnqueueScrapeAsync(string url, CancellationToken cancellationToken = default)
    {
        var job = new ScrapingJob(url);
        await _dbContext.ScrapingJobs.AddAsync(job, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}