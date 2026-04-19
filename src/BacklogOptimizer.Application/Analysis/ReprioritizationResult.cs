namespace BacklogOptimizer.Application.Analysis;

public record ReprioritizationResult(
    int AnalyzedCount,
    int SkippedCount,
    IReadOnlyList<string> Errors);
