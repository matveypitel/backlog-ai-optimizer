using BacklogOptimizer.Core.Common;

namespace BacklogOptimizer.Core.Entities;

public class ReprioritizationSuggestion : BaseEntity
{
    public string JiraKey { get; private set; }
    public string CurrentPriority { get; private set; }
    public string SuggestedPriority { get; private set; }
    public string Reasoning { get; private set; }
    public string CompetitorEvidence { get; private set; }
    public double ConfidenceScore { get; private set; }
    public DateTime AnalyzedAt { get; private set; }

    public ReprioritizationSuggestion(
        string jiraKey,
        string currentPriority,
        string suggestedPriority,
        string reasoning,
        string competitorEvidence,
        double confidenceScore)
    {
        JiraKey = jiraKey;
        CurrentPriority = currentPriority;
        SuggestedPriority = suggestedPriority;
        Reasoning = reasoning;
        CompetitorEvidence = competitorEvidence;
        ConfidenceScore = confidenceScore;
        AnalyzedAt = DateTime.UtcNow;
    }
}
