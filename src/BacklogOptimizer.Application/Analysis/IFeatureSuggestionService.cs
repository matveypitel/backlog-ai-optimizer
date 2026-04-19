using BacklogOptimizer.Core.Common;

namespace BacklogOptimizer.Application.Analysis;

public interface IFeatureSuggestionService
{
    Task<Result<FeatureSuggestionResult>> AnalyzeAsync(CancellationToken cancellationToken = default);
}
