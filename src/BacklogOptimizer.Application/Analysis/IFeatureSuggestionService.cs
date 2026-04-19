using BacklogOptimizer.Core.Common;

namespace BacklogOptimizer.Application.Analysis;

public interface IFeatureSuggestionService
{
    Task<Result> EnqueueAnalysisAsync(CancellationToken cancellationToken = default);
}
