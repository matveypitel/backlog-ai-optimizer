using BacklogOptimizer.Core.Entities;

namespace BacklogOptimizer.UnitTests.Core.Entities;

public class JiraIssueTests
{
    private static JiraIssue CreateIssue(
        string jiraKey = "PROJ-1",
        string projectKey = "PROJ",
        string summary = "Test summary",
        string? description = null,
        string status = "To Do",
        string? priority = "Medium",
        string? assigneeEmail = null,
        string issueType = "Story",
        string? jiraUrl = null) =>
        new(jiraKey, projectKey, summary, description, status, priority,
            assigneeEmail, issueType, jiraUrl,
            DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-1));

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var created = DateTime.UtcNow.AddDays(-10);
        var updated = DateTime.UtcNow.AddDays(-1);

        var issue = new JiraIssue("PROJ-1", "PROJ", "Summary", "Desc",
            "In Progress", "High", "user@example.com", "Bug",
            "https://jira/PROJ-1", created, updated);

        Assert.Equal("PROJ-1", issue.JiraKey);
        Assert.Equal("PROJ", issue.ProjectKey);
        Assert.Equal("Summary", issue.Summary);
        Assert.Equal("Desc", issue.Description);
        Assert.Equal("In Progress", issue.Status);
        Assert.Equal("High", issue.Priority);
        Assert.Equal("user@example.com", issue.AssigneeEmail);
        Assert.Equal("Bug", issue.IssueType);
        Assert.Equal("https://jira/PROJ-1", issue.JiraUrl);
        Assert.Equal(created, issue.JiraCreatedAt);
        Assert.Equal(updated, issue.JiraUpdatedAt);
        Assert.True(issue.LastSyncedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Update_ChangesAllUpdatableProperties()
    {
        var issue = CreateIssue();
        var newUpdated = DateTime.UtcNow;

        issue.Update("New Summary", "New Desc", "Done", "Low",
            "new@example.com", "Task", "https://jira/new", newUpdated);

        Assert.Equal("New Summary", issue.Summary);
        Assert.Equal("New Desc", issue.Description);
        Assert.Equal("Done", issue.Status);
        Assert.Equal("Low", issue.Priority);
        Assert.Equal("new@example.com", issue.AssigneeEmail);
        Assert.Equal("Task", issue.IssueType);
        Assert.Equal("https://jira/new", issue.JiraUrl);
        Assert.Equal(newUpdated, issue.JiraUpdatedAt);
    }

    [Fact]
    public void Update_RefreshesLastSyncedAt()
    {
        var issue = CreateIssue();
        var before = DateTime.UtcNow;

        issue.Update("s", null, "Done", null, null, "Bug", null, DateTime.UtcNow);

        Assert.True(issue.LastSyncedAt >= before);
    }

    [Fact]
    public void SetPriority_ChangesPriority()
    {
        var issue = CreateIssue(priority: "Low");

        issue.SetPriority("Critical");

        Assert.Equal("Critical", issue.Priority);
    }

    [Fact]
    public void SetPriority_NullClearsPriority()
    {
        var issue = CreateIssue(priority: "High");

        issue.SetPriority(null);

        Assert.Null(issue.Priority);
    }
}
