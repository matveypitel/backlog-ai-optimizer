namespace BacklogOptimizer.Application.Auth;

public sealed record UserSummary(Guid Id, string Email, string Role, DateTime CreatedAt);
