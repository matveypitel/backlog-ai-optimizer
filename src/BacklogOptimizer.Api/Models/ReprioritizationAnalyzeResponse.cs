namespace BacklogOptimizer.Api.Models;

public sealed record ReprioritizationAnalyzeResponse(
    int AnalyzedCount,
    int SkippedCount,
    IReadOnlyList<string> Errors);
