namespace BacklogOptimizer.Infrastructure.Analysis.Dto;

internal sealed record FeatureSuggestionLlmItem(
    string Title,
    string Description,
    string IssueType,
    string SuggestedPriority,
    string[] Tags,
    string Reasoning,
    string CompetitorEvidence);

internal sealed record FeatureSuggestionsWrapper(FeatureSuggestionLlmItem[] Suggestions);
