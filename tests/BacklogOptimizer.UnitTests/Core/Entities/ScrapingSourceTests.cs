using BacklogOptimizer.Core.Entities;

namespace BacklogOptimizer.UnitTests.Core.Entities;

public class ScrapingSourceTests
{
    [Fact]
    public void Constructor_SetsUrlAndIsActiveTrue()
    {
        var source = new ScrapingSource("https://example.com", "Example", 24);

        Assert.Equal("https://example.com", source.Url);
        Assert.Equal("Example", source.Name);
        Assert.True(source.IsActive);
        Assert.Equal(24, source.RefreshIntervalHours);
        Assert.Null(source.LastScrapedAt);
    }

    [Fact]
    public void Constructor_WhitespaceName_SetsNameNull()
    {
        var source = new ScrapingSource("https://example.com", "   ");

        Assert.Null(source.Name);
    }

    [Fact]
    public void Constructor_NullName_SetsNameNull()
    {
        var source = new ScrapingSource("https://example.com", null);

        Assert.Null(source.Name);
    }

    [Fact]
    public void Constructor_TrimsName()
    {
        var source = new ScrapingSource("https://example.com", "  Trimmed  ");

        Assert.Equal("Trimmed", source.Name);
    }

    [Fact]
    public void Update_ChangesProperties()
    {
        var source = new ScrapingSource("https://example.com", "Old", 12);

        source.Update("New Name", false, 48);

        Assert.Equal("New Name", source.Name);
        Assert.False(source.IsActive);
        Assert.Equal(48, source.RefreshIntervalHours);
    }

    [Fact]
    public void Update_WhitespaceName_SetsNull()
    {
        var source = new ScrapingSource("https://example.com", "Name", 12);

        source.Update("  ", true, 12);

        Assert.Null(source.Name);
    }

    [Fact]
    public void MarkScraped_SetsLastScrapedAt()
    {
        var source = new ScrapingSource("https://example.com", null);
        var when = DateTime.UtcNow;

        source.MarkScraped(when);

        Assert.Equal(when, source.LastScrapedAt);
    }

    [Fact]
    public void DisplayName_ReturnsName_WhenSet()
    {
        var source = new ScrapingSource("https://example.com", "Example");

        Assert.Equal("Example", source.DisplayName);
    }

    [Fact]
    public void DisplayName_ReturnsUrl_WhenNameNull()
    {
        var source = new ScrapingSource("https://example.com", null);

        Assert.Equal("https://example.com", source.DisplayName);
    }
}
