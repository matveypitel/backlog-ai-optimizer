using BacklogOptimizer.Core.Entities;

namespace BacklogOptimizer.UnitTests.Core.Entities;

public class FeatureSuggestionTests
{
    private static FeatureSuggestion Create() => new(
        "Feature Title",
        "Feature description",
        "Story",
        "High",
        "[\"tag1\"]",
        "Some reasoning",
        "Competitor evidence",
        "Business value",
        "High impact",
        "[\"user story\"]",
        "[\"ac1\"]");

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var before = DateTime.UtcNow;

        var suggestion = Create();

        Assert.Equal("Feature Title", suggestion.Title);
        Assert.Equal("Feature description", suggestion.Description);
        Assert.Equal("Story", suggestion.IssueType);
        Assert.Equal("High", suggestion.SuggestedPriority);
        Assert.Equal("[\"tag1\"]", suggestion.Tags);
        Assert.Equal("Some reasoning", suggestion.Reasoning);
        Assert.Null(suggestion.AppliedAt);
        Assert.Null(suggestion.AppliedJiraKey);
        Assert.True(suggestion.AnalyzedAt >= before);
    }

    [Fact]
    public void MarkApplied_SetsAppliedAtAndJiraKey()
    {
        var suggestion = Create();
        var before = DateTime.UtcNow;

        suggestion.MarkApplied("PROJ-42");

        Assert.NotNull(suggestion.AppliedAt);
        Assert.True(suggestion.AppliedAt >= before);
        Assert.Equal("PROJ-42", suggestion.AppliedJiraKey);
    }

    [Fact]
    public void Update_ChangesAllUpdatableProperties()
    {
        var suggestion = Create();

        suggestion.Update("New desc", "Bug", "Low", "[\"new\"]",
            "New reasoning", "New evidence", "New value",
            "Low impact", "[\"story2\"]", "[\"ac2\"]");

        Assert.Equal("New desc", suggestion.Description);
        Assert.Equal("Bug", suggestion.IssueType);
        Assert.Equal("Low", suggestion.SuggestedPriority);
        Assert.Equal("[\"new\"]", suggestion.Tags);
        Assert.Equal("New reasoning", suggestion.Reasoning);
        Assert.Equal("New evidence", suggestion.CompetitorEvidence);
        Assert.Equal("New value", suggestion.BusinessValue);
        Assert.Equal("Low impact", suggestion.EstimatedImpact);
        Assert.Equal("[\"story2\"]", suggestion.UserStories);
        Assert.Equal("[\"ac2\"]", suggestion.AcceptanceCriteria);
    }

    [Fact]
    public void Update_RefreshesAnalyzedAt()
    {
        var suggestion = Create();
        var before = DateTime.UtcNow;

        suggestion.Update("d", "Bug", "Low", "[]", "r", "e", "v", "i", "[]", "[]");

        Assert.True(suggestion.AnalyzedAt >= before);
    }
}
