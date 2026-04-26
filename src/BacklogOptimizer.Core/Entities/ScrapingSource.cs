using BacklogOptimizer.Core.Common;

namespace BacklogOptimizer.Core.Entities;

public class ScrapingSource : BaseEntity
{
    public string Url { get; private set; }
    public string? Name { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastScrapedAt { get; private set; }
    public int? RefreshIntervalHours { get; private set; }

    private ScrapingSource()
    {
        Url = string.Empty;
    }

    public ScrapingSource(string url, string? name, int? refreshIntervalHours = null)
    {
        Url = url;
        Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        IsActive = true;
        RefreshIntervalHours = refreshIntervalHours;
    }

    public void Update(string? name, bool isActive, int? refreshIntervalHours)
    {
        Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        IsActive = isActive;
        RefreshIntervalHours = refreshIntervalHours;
    }

    public void MarkScraped(DateTime when)
    {
        LastScrapedAt = when;
    }

    public string DisplayName => Name ?? Url;
}
