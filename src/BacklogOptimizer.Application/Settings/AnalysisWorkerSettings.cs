namespace BacklogOptimizer.Application.Settings;

public class AnalysisWorkerSettings
{
    public const string SectionName = "AnalysisWorker";

    public int PollingIntervalSeconds { get; init; } = 5;
}
