using BacklogOptimizer.Core.Entities;

namespace BacklogOptimizer.UnitTests.Core.Entities;

public class CompetitorFeatureTests
{
    private static CompetitorFeature Create() =>
        new(Guid.NewGuid(), "Feature Name", "A description",
            "Analytics", "[\"benefit1\"]", "[\"use1\"]",
            "Differentiator text", "Enterprise");

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var pageId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        var feature = new CompetitorFeature(pageId, "Name", "Desc",
            "Category", "[\"b\"]", "[\"u\"]", "diff", "audience");

        Assert.Equal(pageId, feature.ScrapedPageId);
        Assert.Equal("Name", feature.Name);
        Assert.Equal("Desc", feature.Description);
        Assert.Equal("Category", feature.Category);
        Assert.Equal("[\"b\"]", feature.KeyBenefits);
        Assert.Equal("[\"u\"]", feature.UseCases);
        Assert.Equal("diff", feature.Differentiators);
        Assert.Equal("audience", feature.TargetAudience);
        Assert.True(feature.ExtractedAt >= before);
    }

    [Fact]
    public void Constructor_NullCategoryAndAudience_SetsNull()
    {
        var feature = new CompetitorFeature(Guid.NewGuid(), "Name", "Desc",
            null, "[]", "[]", "diff", null);

        Assert.Null(feature.Category);
        Assert.Null(feature.TargetAudience);
    }

    [Fact]
    public void Update_ChangesProperties()
    {
        var feature = Create();
        var before = DateTime.UtcNow;

        feature.Update("New desc", "New cat", "[\"b2\"]", "[\"u2\"]", "New diff", "New audience");

        Assert.Equal("New desc", feature.Description);
        Assert.Equal("New cat", feature.Category);
        Assert.Equal("[\"b2\"]", feature.KeyBenefits);
        Assert.Equal("[\"u2\"]", feature.UseCases);
        Assert.Equal("New diff", feature.Differentiators);
        Assert.Equal("New audience", feature.TargetAudience);
        Assert.True(feature.ExtractedAt >= before);
    }
}
