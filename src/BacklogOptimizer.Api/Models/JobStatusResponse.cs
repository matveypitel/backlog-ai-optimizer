namespace BacklogOptimizer.Api.Models;

public sealed record JobStatusResponse(
    Guid Id,
    string Status,
    string? ErrorMessage,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
