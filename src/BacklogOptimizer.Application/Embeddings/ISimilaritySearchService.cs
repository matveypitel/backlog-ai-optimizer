using BacklogOptimizer.Core.Common;

namespace BacklogOptimizer.Application.Embeddings;

public interface ISimilaritySearchService
{
    Task<Result<IReadOnlyList<SimilarityMatch>>> FindSimilarFeaturesAsync(
        string jiraKey,
        int topN,
        CancellationToken cancellationToken = default);
}
