using BacklogOptimizer.Core.Entities;

namespace BacklogOptimizer.UnitTests.Core.Entities;

public class ScrapedPageTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var page = new ScrapedPage("https://example.com", "<html/>", "Title", "Extracted text");

        Assert.Equal("https://example.com", page.Url);
        Assert.Equal("<html/>", page.HtmlContent);
        Assert.Equal("Title", page.Title);
        Assert.Equal("Extracted text", page.ExtractedContent);
        Assert.Empty(page.Features);
    }

    [Fact]
    public void Constructor_NullTitleAndContent()
    {
        var page = new ScrapedPage("https://example.com", "<html/>", null, null);

        Assert.Null(page.Title);
        Assert.Null(page.ExtractedContent);
    }

    [Fact]
    public void Update_ChangesProperties()
    {
        var page = new ScrapedPage("https://example.com", "<old/>", "Old", "old text");

        page.Update("<new/>", "New", "new text");

        Assert.Equal("<new/>", page.HtmlContent);
        Assert.Equal("New", page.Title);
        Assert.Equal("new text", page.ExtractedContent);
    }
}
