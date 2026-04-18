namespace BacklogOptimizer.Api.Models;

public sealed record EmbeddingSearchRequest(string JiraKey, int TopN = 5);
