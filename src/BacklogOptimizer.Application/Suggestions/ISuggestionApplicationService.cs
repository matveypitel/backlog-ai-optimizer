using BacklogOptimizer.Core.Common;

namespace BacklogOptimizer.Application.Suggestions;

public interface ISuggestionApplicationService
{
    Task<Result> ApplyReprioritizationAsync(string jiraKey, CancellationToken cancellationToken = default);

    Task<Result<ApplyFeatureSuggestionResult>> ApplyFeatureSuggestionAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed record ApplyFeatureSuggestionResult(string JiraKey);
