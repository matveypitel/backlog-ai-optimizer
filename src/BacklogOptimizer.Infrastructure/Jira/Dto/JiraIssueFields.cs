using System.Text.Json;

namespace BacklogOptimizer.Infrastructure.Jira.Dto;

internal sealed record JiraIssueFields(
    string Summary,
    JsonElement? Description,
    JiraStatusDto Status,
    JiraNamedFieldDto? Priority,
    JiraAssigneeDto? Assignee,
    JiraNamedFieldDto IssueType,
    string Created,
    string Updated);
