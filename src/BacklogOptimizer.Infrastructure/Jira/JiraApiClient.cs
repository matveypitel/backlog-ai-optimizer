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

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Jira API returned {(int)response.StatusCode}: {responseBody}");
        }

        return await response.Content
            .ReadFromJsonAsync<JiraSearchResponse>(ReadOptions, cancellationToken)
            ?? throw new InvalidOperationException("Empty response from Jira search API");
    }

    private sealed record JiraSearchRequest(
        string Jql,
        int MaxResults,
        string? NextPageToken,
        string[] Fields);
}
