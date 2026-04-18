namespace BacklogOptimizer.Application.Settings;

public class ScrapingWorkerSettings
{
    public const string SectionName = "ScrapingWorker";

    public int PollingIntervalSeconds { get; init; } = 5;
}