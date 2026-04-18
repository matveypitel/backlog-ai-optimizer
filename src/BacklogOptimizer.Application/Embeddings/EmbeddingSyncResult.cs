namespace BacklogOptimizer.Application.Embeddings;

public record EmbeddingSyncResult(int EmbeddedCount, int SkippedCount, IReadOnlyList<string> Errors);
