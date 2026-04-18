namespace BacklogOptimizer.Application.Embeddings;

public interface ISimilaritySearchService
{
    Task<IReadOnlyList<SimilarityMatch>> FindSimilarPagesAsync(
        string jiraKey,
        int topN,
        CancellationToken cancellationToken = default);
}
