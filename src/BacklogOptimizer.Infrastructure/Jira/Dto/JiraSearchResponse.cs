namespace BacklogOptimizer.Infrastructure.Jira.Dto;

internal sealed record JiraSearchResponse(
    List<JiraIssueDto> Issues,
    string? NextPageToken);
