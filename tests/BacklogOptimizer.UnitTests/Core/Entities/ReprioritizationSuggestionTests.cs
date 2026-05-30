using BacklogOptimizer.Core.Entities;

namespace BacklogOptimizer.UnitTests.Core.Entities;

public class ReprioritizationSuggestionTests
{
    private static ReprioritizationSuggestion Create() =>
        new("PROJ-1", "Low", "High", "Strong evidence", "Competitor X does it", 0.9);

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var before = DateTime.UtcNow;

        var suggestion = Create();

        Assert.Equal("PROJ-1", suggestion.JiraKey);
        Assert.Equal("Low", suggestion.CurrentPriority);
        Assert.Equal("High", suggestion.SuggestedPriority);
        Assert.Equal("Strong evidence", suggestion.Reasoning);
        Assert.Equal("Competitor X does it", suggestion.CompetitorEvidence);
        Assert.Equal(0.9, suggestion.ConfidenceScore);
        Assert.Null(suggestion.AppliedAt);
        Assert.True(suggestion.AnalyzedAt >= before);
    }

    [Fact]
    public void MarkApplied_SetsAppliedAt()
    {
        var suggestion = Create();
        var before = DateTime.UtcNow;

        suggestion.MarkApplied();

        Assert.NotNull(suggestion.AppliedAt);
        Assert.True(suggestion.AppliedAt >= before);
    }

    [Fact]
    public void Update_ChangesAllUpdatableProperties()
    {
        var suggestion = Create();
        var before = DateTime.UtcNow;

        suggestion.Update("Medium", "Critical", "New reason", "New evidence", 0.75);

        Assert.Equal("Medium", suggestion.CurrentPriority);
        Assert.Equal("Critical", suggestion.SuggestedPriority);
        Assert.Equal("New reason", suggestion.Reasoning);
        Assert.Equal("New evidence", suggestion.CompetitorEvidence);
        Assert.Equal(0.75, suggestion.ConfidenceScore);
        Assert.True(suggestion.AnalyzedAt >= before);
    }
}
