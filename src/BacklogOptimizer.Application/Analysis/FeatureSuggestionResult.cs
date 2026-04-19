namespace BacklogOptimizer.Application.Analysis;

public record FeatureSuggestionResult(
    int GapPagesFound,
    int SuggestionsGenerated,
    IReadOnlyList<string> Errors);
