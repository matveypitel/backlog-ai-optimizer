using BacklogOptimizer.Core.Entities;

namespace BacklogOptimizer.Application.Prompts;

public sealed record PromptTemplateView(
    PromptType Type,
    string Instructions,
    string Schema,
    string DefaultInstructions,
    DateTime? UpdatedAt,
    string? UpdatedByUserId);
