namespace BacklogOptimizer.Application.Embeddings;

public record SimilarityMatch(
    Guid CompetitorFeatureId,
    string Name,
    string? Category,
    Guid ScrapedPageId,
    string Url,
    double Score);
