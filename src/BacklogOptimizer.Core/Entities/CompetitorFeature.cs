using BacklogOptimizer.Core.Common;

namespace BacklogOptimizer.Core.Entities;

public class CompetitorFeature : BaseEntity
{
    public Guid ScrapedPageId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string? Category { get; private set; }
    public DateTime ExtractedAt { get; private set; }

    public ScrapedPage ScrapedPage { get; private set; } = null!;
    public CompetitorFeatureEmbedding? Embedding { get; private set; }

    private CompetitorFeature() { Name = string.Empty; Description = string.Empty; }

    public CompetitorFeature(Guid scrapedPageId, string name, string description, string? category)
    {
        ScrapedPageId = scrapedPageId;
        Name = name;
        Description = description;
        Category = category;
        ExtractedAt = DateTime.UtcNow;
    }

    public void Update(string description, string? category)
    {
        Description = description;
        Category = category;
        ExtractedAt = DateTime.UtcNow;
    }
}
