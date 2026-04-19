namespace BacklogOptimizer.Application.Analysis;

public interface IFeatureSuggestionService
{
    Task<FeatureSuggestionResult> AnalyzeAsync(CancellationToken cancellationToken = default);
}
