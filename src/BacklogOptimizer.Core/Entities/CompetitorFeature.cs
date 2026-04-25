using BacklogOptimizer.Core.Common;

namespace BacklogOptimizer.Core.Entities;

public class CompetitorFeature : BaseEntity
{
    public Guid ScrapedPageId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string? Category { get; private set; }
    public string KeyBenefits { get; private set; }
    public string UseCases { get; private set; }
    public string Differentiators { get; private set; }
    public string? TargetAudience { get; private set; }
    public DateTime ExtractedAt { get; private set; }

    public ScrapedPage ScrapedPage { get; private set; } = null!;
    public CompetitorFeatureEmbedding? Embedding { get; private set; }

    private CompetitorFeature()
    {
        Name = string.Empty;
        Description = string.Empty;
        KeyBenefits = "[]";
        UseCases = "[]";
        Differentiators = string.Empty;
    }

    public CompetitorFeature(
        Guid scrapedPageId,
        string name,
        string description,
        string? category,
        string keyBenefits,
        string useCases,
        string differentiators,
        string? targetAudience)
    {
        ScrapedPageId = scrapedPageId;
        Name = name;
        Description = description;
        Category = category;
        KeyBenefits = keyBenefits;
        UseCases = useCases;
        Differentiators = differentiators;
        TargetAudience = targetAudience;
        ExtractedAt = DateTime.UtcNow;
    }

    public void Update(
        string description,
        string? category,
        string keyBenefits,
        string useCases,
        string differentiators,
        string? targetAudience)
    {
        Description = description;
        Category = category;
        KeyBenefits = keyBenefits;
        UseCases = useCases;
        Differentiators = differentiators;
        TargetAudience = targetAudience;
        ExtractedAt = DateTime.UtcNow;
    }
}
