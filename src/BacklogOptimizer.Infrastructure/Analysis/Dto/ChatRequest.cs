namespace BacklogOptimizer.Infrastructure.Analysis.Dto;

internal sealed record ChatRequest(
    string Model,
    ChatMessage[] Messages,
    ChatResponseFormat ResponseFormat,
    double Temperature = 0.2);

internal sealed record ChatMessage(string Role, string Content);

internal sealed record ChatResponseFormat(string Type);
