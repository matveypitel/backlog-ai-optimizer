using BacklogOptimizer.Core.Common;

namespace BacklogOptimizer.Application.Scraping;

public interface IScrapingService
{
    Task<Result> EnqueueScrapeAsync(string url, CancellationToken cancellationToken = default);
}
