using BacklogOptimizer.Core.Common;

namespace BacklogOptimizer.Core.Entities;

public class JiraIssue : BaseEntity
{
    public string JiraKey { get; private set; }
    public string ProjectKey { get; private set; }
    public string Summary { get; private set; }
    public string? Description { get; private set; }
    public string Status { get; private set; }
    public string? Priority { get; private set; }
    public string? AssigneeEmail { get; private set; }
    public string IssueType { get; private set; }
    public DateTime JiraCreatedAt { get; private set; }
    public DateTime JiraUpdatedAt { get; private set; }
    public DateTime LastSyncedAt { get; private set; }

    public JiraIssue(string jiraKey, string projectKey, string summary,
        string? description, string status, string? priority,
        string? assigneeEmail, string issueType,
        DateTime jiraCreatedAt, DateTime jiraUpdatedAt)
    {
        JiraKey = jiraKey;
        ProjectKey = projectKey;
        Summary = summary;
        Description = description;
        Status = status;
        Priority = priority;
        AssigneeEmail = assigneeEmail;
        IssueType = issueType;
        JiraCreatedAt = jiraCreatedAt;
        JiraUpdatedAt = jiraUpdatedAt;
        LastSyncedAt = DateTime.UtcNow;
    }

    public void Update(string summary, string? description, string status,
        string? priority, string? assigneeEmail, string issueType,
        DateTime jiraUpdatedAt)
    {
        Summary = summary;
        Description = description;
        Status = status;
        Priority = priority;
        AssigneeEmail = assigneeEmail;
        IssueType = issueType;
        JiraUpdatedAt = jiraUpdatedAt;
        LastSyncedAt = DateTime.UtcNow;
    }
}
