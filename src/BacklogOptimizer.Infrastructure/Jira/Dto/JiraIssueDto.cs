namespace BacklogOptimizer.Infrastructure.Jira.Dto;

internal sealed record JiraIssueDto(
    string Id,
    string Key,
    JiraIssueFields Fields);
