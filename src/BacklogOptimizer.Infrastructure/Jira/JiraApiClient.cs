using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using BacklogOptimizer.Infrastructure.Jira.Dto;

namespace BacklogOptimizer.Infrastructure.Jira;

internal sealed class JiraApiClient
{
    private static readonly JsonSerializerOptions ReadOptions = new() { PropertyNameCaseInsensitive = true };

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;

    public JiraApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<JiraSearchResponse> SearchAsync(
        string jql, string? nextPageToken, int maxResults, CancellationToken cancellationToken)
    {
        var body = new JiraSearchRequest(jql, maxResults, nextPageToken,
            ["summary", "description", "status", "priority", "assignee", "issuetype", "created", "updated"]);

        var response = await _httpClient.PostAsJsonAsync(
            "/rest/api/3/search/jql", body, WriteOptions, cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content
            .ReadFromJsonAsync<JiraSearchResponse>(ReadOptions, cancellationToken)
            ?? throw new InvalidOperationException("Empty response from Jira search API");
    }

    public async Task UpdatePriorityAsync(string jiraKey, string priorityName, CancellationToken cancellationToken)
    {
        var body = new
        {
            fields = new
            {
                priority = new { name = priorityName }
            }
        };

        var response = await _httpClient.PutAsJsonAsync(
            $"/rest/api/3/issue/{jiraKey}", body, WriteOptions, cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<string> CreateIssueAsync(
        string projectKey, string summary, string? descriptionPlainText,
        string issueTypeName, string priorityName, CancellationToken cancellationToken)
    {
        var fields = new Dictionary<string, object?>
        {
            ["project"] = new { key = projectKey },
            ["summary"] = summary,
            ["issuetype"] = new { name = issueTypeName },
            ["priority"] = new { name = priorityName }
        };

        if (!string.IsNullOrWhiteSpace(descriptionPlainText))
        {
            fields["description"] = new
            {
                type = "doc",
                version = 1,
                content = new object[]
                {
                    new
                    {
                        type = "paragraph",
                        content = new object[]
                        {
                            new { type = "text", text = descriptionPlainText }
                        }
                    }
                }
            };
        }

        var body = new { fields };

        var response = await _httpClient.PostAsJsonAsync(
            "/rest/api/3/issue", body, WriteOptions, cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        var created = await response.Content
            .ReadFromJsonAsync<JiraCreateIssueResponse>(ReadOptions, cancellationToken)
            ?? throw new InvalidOperationException("Empty response from Jira create-issue API");

        return created.Key;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode) return;

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException($"Jira API returned {(int)response.StatusCode}: {responseBody}");
    }

    private sealed record JiraSearchRequest(
        string Jql,
        int MaxResults,
        string? NextPageToken,
        string[] Fields);

    private sealed record JiraCreateIssueResponse(string Id, string Key, string Self);
}
