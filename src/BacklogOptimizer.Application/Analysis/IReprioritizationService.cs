namespace BacklogOptimizer.Application.Analysis;

public interface IReprioritizationService
{
    Task<ReprioritizationResult> AnalyzeAsync(CancellationToken cancellationToken = default);
}
