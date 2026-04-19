using BacklogOptimizer.Core.Common;

namespace BacklogOptimizer.Application.Embeddings;

public interface ISimilaritySearchService
{
    Task<Result<IReadOnlyList<SimilarityMatch>>> FindSimilarPagesAsync(
        string jiraKey,
        int topN,
        CancellationToken cancellationToken = default);
}
