using BacklogOptimizer.Core.Common;

namespace BacklogOptimizer.Core.Entities;

public class ScrapedPage : BaseEntity
{
    public string Url { get; private set; }
    public string HtmlContent { get; private set; }
    public string? Title { get; private set; }
    public string? ExtractedContent { get; private set; }

    public ScrapedPage(string url, string htmlContent, string? title, string? extractedContent)
    {
        Url = url;
        HtmlContent = htmlContent;
        Title = title;
        ExtractedContent = extractedContent;
    }

    public void Update(string htmlContent, string? title, string? extractedContent)
    {
        HtmlContent = htmlContent;
        Title = title;
        ExtractedContent = extractedContent;
    }
}
