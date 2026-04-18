namespace BacklogOptimizer.Application.Embeddings;

public class OpenAiSettings
{
    public const string SectionName = "OpenAi";

    public string ApiKey { get; init; } = string.Empty;
    public string EmbeddingModel { get; init; } = "text-embedding-3-small";
    public string[] CompletedStatuses { get; init; } = ["Done", "Closed", "Resolved"];
}
