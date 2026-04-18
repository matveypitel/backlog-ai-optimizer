namespace BacklogOptimizer.Application.Scraping;

public class ScrapingSettings
{
    public const string SectionName = "Scraping";

    public bool Headless { get; init; } = true;
    public int Timeout { get; init; } = 100000;
    public int MaxRetryAttempts { get; init; } = 3;
    public int MaxRetryDelay { get; init; } = 5000;
}
