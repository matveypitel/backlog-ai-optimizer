namespace BacklogOptimizer.Application.Auth;

public sealed class DemoUserSeedSettings
{
    public const string SectionName = "DemoUserSeed";

    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
