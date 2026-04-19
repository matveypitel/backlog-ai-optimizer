namespace BacklogOptimizer.Infrastructure.Analysis.Dto;

internal sealed record ChatResponse(ChatChoice[] Choices);

internal sealed record ChatChoice(ChatResponseMessage Message);

internal sealed record ChatResponseMessage(string Content);
