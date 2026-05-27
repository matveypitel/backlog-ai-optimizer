using System.Globalization;

using BacklogOptimizer.Application.Embeddings;
using BacklogOptimizer.Application.Jira;
using BacklogOptimizer.Core.Common;
using BacklogOptimizer.Core.Entities;
using BacklogOptimizer.Infrastructure.Jira.Dto;
using BacklogOptimizer.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BacklogOptimizer.Infrastructure.Jira;

internal sealed class JiraSyncService : IJiraSyncService
{
    private readonly JiraApiClient _apiClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly IEmbeddingSyncService _embeddingSyncService;
    private readonly JiraSettings _settings;
    private readonly ILogger<JiraSyncService> _logger;

    public JiraSyncService(
        JiraApiClient apiClient,
        ApplicationDbContext dbContext,
        IEmbeddingSyncService embeddingSyncService,
        IOptions<JiraSettings> settings,
        ILogger<JiraSyncService> logger)
    {
        _apiClient = apiClient;
        _dbContext = dbContext;
        _embeddingSyncService = embeddingSyncService;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<Result<JiraSyncResult>> SyncAsync(CancellationToken cancellationToken = default)
    {
        var jql = _settings.JqlFilter
            ?? $"project = {_settings.ProjectKey} AND issueType in standardIssueTypes() ORDER BY created ASC";

        var errors = new List<string>();
        var syncedCount = 0;
        string? nextPageToken = null;
        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var syncFullyCompleted = false;

        while (true)
        {
            JiraSearchResponse page;
            try
            {
                page = await _apiClient.SearchAsync(jql, nextPageToken, _settings.MaxResultsPerPage, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Jira API call failed");
                if (syncedCount == 0)
                    return Errors.Jira.ApiFailure(ex.Message);
                errors.Add($"API error: {ex.Message}");
                break;
            }

            foreach (var dto in page.Issues)
            {
                seenKeys.Add(dto.Key);
                try
                {
                    await UpsertAsync(dto, cancellationToken);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    syncedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to sync issue {Key}", dto.Key);
                    errors.Add($"{dto.Key}: {ex.Message}");
                    _dbContext.ChangeTracker.Clear();
                }
            }

            if (string.IsNullOrEmpty(page.NextPageToken) || page.Issues.Count == 0)
            {
                syncFullyCompleted = true;
                break;
            }

            nextPageToken = page.NextPageToken;
        }

        // Delete issues that no longer exist in Jira (only when all pages were fetched successfully)
        if (syncFullyCompleted && seenKeys.Count > 0)
        {
            var deletedCount = 0;
            try
            {
                var orphaned = await _dbContext.JiraIssues
                    .Where(j => j.ProjectKey == _settings.ProjectKey && !seenKeys.Contains(j.JiraKey))
                    .ToListAsync(cancellationToken);

                if (orphaned.Count > 0)
                {
                    _dbContext.JiraIssues.RemoveRange(orphaned);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    deletedCount = orphaned.Count;
                    _logger.LogInformation("Deleted {Count} Jira issues no longer present in Jira: {Keys}",
                        deletedCount, string.Join(", ", orphaned.Select(j => j.JiraKey)));
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to delete orphaned Jira issues");
                errors.Add($"cleanup: {ex.Message}");
            }
        }

        if (syncedCount > 0)
        {
            try
            {
                var embeddingResult = await _embeddingSyncService.SyncJiraEmbeddingsAsync(cancellationToken);
                if (embeddingResult.IsSuccess && embeddingResult.Value!.Errors.Count > 0)
                {
                    foreach (var err in embeddingResult.Value.Errors)
                        errors.Add($"embedding {err}");
                }
                else if (!embeddingResult.IsSuccess)
                {
                    errors.Add($"embedding: {embeddingResult.Error!.Description}");
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Auto-embedding after Jira sync failed");
                errors.Add($"embedding: {ex.Message}");
            }
        }

        return new JiraSyncResult(syncedCount, errors.AsReadOnly());
    }

    private async Task UpsertAsync(JiraIssueDto dto, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.JiraIssues
            .FirstOrDefaultAsync(j => j.JiraKey == dto.Key, cancellationToken);

        var f = dto.Fields;
        var description = f.Description?.GetRawText();
        var created = DateTimeOffset.Parse(f.Created, CultureInfo.InvariantCulture).UtcDateTime;
        var updated = DateTimeOffset.Parse(f.Updated, CultureInfo.InvariantCulture).UtcDateTime;
        var jiraUrl = $"{_settings.BaseUrl.TrimEnd('/')}/browse/{dto.Key}";

        if (existing is not null)
        {
            existing.Update(f.Summary, description, f.Status.Name,
                f.Priority?.Name, f.Assignee?.EmailAddress,
                f.IssueType.Name, jiraUrl, updated);
        }
        else
        {
            await _dbContext.JiraIssues.AddAsync(new JiraIssue(
                dto.Key, _settings.ProjectKey, f.Summary, description,
                f.Status.Name, f.Priority?.Name, f.Assignee?.EmailAddress,
                f.IssueType.Name, jiraUrl, created, updated),
                cancellationToken);
        }
    }
}
