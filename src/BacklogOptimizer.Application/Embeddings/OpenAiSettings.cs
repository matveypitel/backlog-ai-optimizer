namespace BacklogOptimizer.Application.Embeddings;

public class OpenAiSettings
{
    public const string SectionName = "OpenAi";

    public string ApiKey { get; init; } = string.Empty;
    public string EmbeddingModel { get; init; } = "text-embedding-3-small";
    public string[] CompletedStatuses { get; init; } = ["Done", "Closed", "Resolved"];
    public string CompletionModel { get; init; } = "gpt-4o-mini";
    public double CoverageGapThreshold { get; init; } = 0.7;
    public int ReprioritizationSimilarFeaturesTopN { get; init; } = 3;
    public double ReprioritizationSimilarityThreshold { get; init; } = 0.3;
    public int FeatureSuggestionBatchSize { get; init; } = 15;
    public int ReprioritizationBatchSize { get; init; } = 15;
}
