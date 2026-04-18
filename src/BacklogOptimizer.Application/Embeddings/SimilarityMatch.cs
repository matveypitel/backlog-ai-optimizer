namespace BacklogOptimizer.Application.Embeddings;

public record SimilarityMatch(Guid ScrapedPageId, string Url, string? Title, double Score);
