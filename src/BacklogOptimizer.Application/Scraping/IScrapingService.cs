namespace BacklogOptimizer.Application.Scraping;

public interface IScrapingService
{
    Task EnqueueScrapeAsync(string url, CancellationToken cancellationToken = default);
}