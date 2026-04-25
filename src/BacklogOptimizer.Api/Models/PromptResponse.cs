using BacklogOptimizer.Core.Entities;

namespace BacklogOptimizer.Api.Models;

public sealed record PromptResponse(
    PromptType Type,
    string Instructions,
    string Schema,
    string DefaultInstructions,
    DateTime? UpdatedAt,
    string? UpdatedByUserId);
