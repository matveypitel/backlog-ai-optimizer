using BacklogOptimizer.Application.Jira;
using BacklogOptimizer.Application.Suggestions;
using BacklogOptimizer.Core.Common;
using BacklogOptimizer.Infrastructure.Jira;
using BacklogOptimizer.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BacklogOptimizer.Infrastructure.Suggestions;

internal sealed class SuggestionApplicationService : ISuggestionApplicationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly JiraApiClient _jiraApiClient;
    private readonly JiraSettings _jiraSettings;
    private readonly ILogger<SuggestionApplicationService> _logger;

    public SuggestionApplicationService(
        ApplicationDbContext dbContext,
        JiraApiClient jiraApiClient,
        IOptions<JiraSettings> jiraSettings,
        ILogger<SuggestionApplicationService> logger)
    {
        _dbContext = dbContext;
        _jiraApiClient = jiraApiClient;
        _jiraSettings = jiraSettings.Value;
        _logger = logger;
    }

    public async Task<Result> ApplyReprioritizationAsync(string jiraKey, CancellationToken cancellationToken = default)
    {
        var suggestion = await _dbContext.ReprioritizationSuggestions
            .FirstOrDefaultAsync(s => s.JiraKey == jiraKey && s.AppliedAt == null, cancellationToken);

        if (suggestion is null)
            return Errors.NotFound.ReprioritizationSuggestion(jiraKey);

        try
        {
            await _jiraApiClient.UpdatePriorityAsync(jiraKey, suggestion.SuggestedPriority, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to update Jira priority for {JiraKey}", jiraKey);
            return Errors.Jira.ApiFailure(ex.Message);
        }

        var localIssue = await _dbContext.JiraIssues
            .FirstOrDefaultAsync(j => j.JiraKey == jiraKey, cancellationToken);
        localIssue?.SetPriority(suggestion.SuggestedPriority);

        suggestion.MarkApplied();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<ApplyFeatureSuggestionResult>> ApplyFeatureSuggestionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var suggestion = await _dbContext.FeatureSuggestions
            .FirstOrDefaultAsync(s => s.Id == id && s.AppliedAt == null, cancellationToken);

        if (suggestion is null)
            return Errors.NotFound.FeatureSuggestion(id);

        string createdKey;
        try
        {
            createdKey = await _jiraApiClient.CreateIssueAsync(
                _jiraSettings.ProjectKey,
                suggestion.Title,
                suggestion.Description,
                suggestion.IssueType,
                suggestion.SuggestedPriority,
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to create Jira issue for feature suggestion {Id}", id);
            return Errors.Jira.ApiFailure(ex.Message);
        }

        suggestion.MarkApplied(createdKey);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApplyFeatureSuggestionResult(createdKey);
    }
}
