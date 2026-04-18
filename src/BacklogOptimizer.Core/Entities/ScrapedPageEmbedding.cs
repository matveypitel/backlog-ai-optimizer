using Pgvector;

namespace BacklogOptimizer.Core.Entities;

public class ScrapedPageEmbedding
{
    public Guid ScrapedPageId { get; private set; }
    public string ModelName { get; private set; } = string.Empty;
    public Vector Vector { get; private set; } = null!;
    public DateTime EmbeddedAt { get; private set; }

    public ScrapedPage ScrapedPage { get; private set; } = null!;

    private ScrapedPageEmbedding() { }

    public ScrapedPageEmbedding(Guid scrapedPageId, string modelName, Vector vector)
    {
        ScrapedPageId = scrapedPageId;
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
