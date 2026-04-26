namespace BacklogOptimizer.Api.Models;

public sealed record JiraIssueResponse(
    Guid Id,
    string JiraKey,
    string Summary,
    string? Description,
    string Status,
    string? Priority,
    string IssueType,
    string? AssigneeEmail,
    DateTime JiraCreatedAt,
    DateTime JiraUpdatedAt,
    DateTime LastSyncedAt);
