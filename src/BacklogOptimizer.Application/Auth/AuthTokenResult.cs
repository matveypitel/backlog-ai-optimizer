namespace BacklogOptimizer.Application.Auth;

public record AuthTokenResult(string AccessToken, DateTime ExpiresAtUtc, string Email, string Role);
