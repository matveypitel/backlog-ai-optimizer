namespace BacklogOptimizer.Api.Models;

public sealed record FeatureSuggestionAnalyzeResponse(
    int GapPagesFound,
    int SuggestionsGenerated,
    IReadOnlyList<string> Errors);
