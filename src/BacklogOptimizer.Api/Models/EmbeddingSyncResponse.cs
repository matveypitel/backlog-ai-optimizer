namespace BacklogOptimizer.Api.Models;

public sealed record EmbeddingSyncResponse(int EmbeddedCount, int SkippedCount, IReadOnlyList<string> Errors);
