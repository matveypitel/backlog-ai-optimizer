using System.Diagnostics.CodeAnalysis;

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
    public string BusinessValue { get; private set; }
    public string EstimatedImpact { get; private set; }
    public string UserStories { get; private set; }
    public string AcceptanceCriteria { get; private set; }
    public DateTime AnalyzedAt { get; private set; }
    public DateTime? AppliedAt { get; private set; }
    public string? AppliedJiraKey { get; private set; }

    public void MarkApplied(string jiraKey)
    {
        AppliedAt = DateTime.UtcNow;
        AppliedJiraKey = jiraKey;
    }

    [ExcludeFromCodeCoverage]
    private FeatureSuggestion()
    {
        Title = string.Empty;
        Description = string.Empty;
        IssueType = string.Empty;
        SuggestedPriority = string.Empty;
        Tags = "[]";
        Reasoning = string.Empty;
        CompetitorEvidence = string.Empty;
        BusinessValue = string.Empty;
        EstimatedImpact = string.Empty;
        UserStories = "[]";
        AcceptanceCriteria = "[]";
    }

    public FeatureSuggestion(
        string title,
        string description,
        string issueType,
        string suggestedPriority,
        string tags,
        string reasoning,
        string competitorEvidence,
        string businessValue,
        string estimatedImpact,
        string userStories,
        string acceptanceCriteria)
    {
        Title = title;
        Description = description;
        IssueType = issueType;
        SuggestedPriority = suggestedPriority;
        Tags = tags;
        Reasoning = reasoning;
        CompetitorEvidence = competitorEvidence;
        BusinessValue = businessValue;
        EstimatedImpact = estimatedImpact;
        UserStories = userStories;
        AcceptanceCriteria = acceptanceCriteria;
        AnalyzedAt = DateTime.UtcNow;
    }

    public void Update(
        string description,
        string issueType,
        string suggestedPriority,
        string tags,
        string reasoning,
        string competitorEvidence,
        string businessValue,
        string estimatedImpact,
        string userStories,
        string acceptanceCriteria)
    {
        Description = description;
        IssueType = issueType;
        SuggestedPriority = suggestedPriority;
        Tags = tags;
        Reasoning = reasoning;
        CompetitorEvidence = competitorEvidence;
        BusinessValue = businessValue;
        EstimatedImpact = estimatedImpact;
        UserStories = userStories;
        AcceptanceCriteria = acceptanceCriteria;
        AnalyzedAt = DateTime.UtcNow;
    }
}
