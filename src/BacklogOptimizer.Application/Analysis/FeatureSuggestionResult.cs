namespace BacklogOptimizer.Application.Analysis;

public record FeatureSuggestionResult(
    int GapFeaturesFound,
    int SuggestionsGenerated,
    IReadOnlyList<string> Errors);
