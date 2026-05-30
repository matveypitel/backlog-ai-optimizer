using System.Diagnostics.CodeAnalysis;

using Pgvector;

namespace BacklogOptimizer.Core.Entities;

public class CompetitorFeatureEmbedding
{
    public Guid CompetitorFeatureId { get; private set; }
    public string ModelName { get; private set; } = string.Empty;
    public Vector Vector { get; private set; } = null!;
    public DateTime EmbeddedAt { get; private set; }

    public CompetitorFeature CompetitorFeature { get; private set; } = null!;

    [ExcludeFromCodeCoverage]
    private CompetitorFeatureEmbedding() { }

    public CompetitorFeatureEmbedding(Guid competitorFeatureId, string modelName, Vector vector)
    {
        CompetitorFeatureId = competitorFeatureId;
        ModelName = modelName;
        Vector = vector;
        EmbeddedAt = DateTime.UtcNow;
    }

    public void Update(string modelName, Vector vector)
    {
        ModelName = modelName;
        Vector = vector;
        EmbeddedAt = DateTime.UtcNow;
    }
}
