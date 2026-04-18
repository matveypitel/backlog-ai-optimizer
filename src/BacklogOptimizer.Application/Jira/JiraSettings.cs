namespace BacklogOptimizer.Application.Jira;

public class JiraSettings
{
    public const string SectionName = "Jira";

    public string BaseUrl { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string ApiToken { get; init; } = string.Empty;
    public string ProjectKey { get; init; } = string.Empty;
    public string? JqlFilter { get; init; }
    public int MaxResultsPerPage { get; init; } = 50;
}
