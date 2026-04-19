using BacklogOptimizer.Core.Common;

namespace BacklogOptimizer.Core.Entities;

public class FeatureSuggestion : BaseEntity
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string IssueType { get; private set; }
    public string SuggestedPriority { get; private set; }
    public string Tags { get; private set; }
    public string Reasoning { get; private set; }
    public string CompetitorEvidence { get; private set; }
    public DateTime AnalyzedAt { get; private set; }

    public FeatureSuggestion(
        string title,
        string description,
        string issueType,
        string suggestedPriority,
        string tags,
        string reasoning,
        string competitorEvidence)
    {
        Title = title;
        Description = description;
        IssueType = issueType;
        SuggestedPriority = suggestedPriority;
        Tags = tags;
        Reasoning = reasoning;
        CompetitorEvidence = competitorEvidence;
        AnalyzedAt = DateTime.UtcNow;
    }
}
