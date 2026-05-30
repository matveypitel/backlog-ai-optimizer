using BacklogOptimizer.Application.Auth;
using BacklogOptimizer.Core.Entities;
using BacklogOptimizer.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BacklogOptimizer.Infrastructure.Auth;

internal sealed class AdminSeeder : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AdminSeedSettings _adminSettings;
    private readonly DemoUserSeedSettings _demoSettings;
    private readonly ILogger<AdminSeeder> _logger;

    public AdminSeeder(
        IServiceScopeFactory scopeFactory,
        IOptions<AdminSeedSettings> adminOptions,
        IOptions<DemoUserSeedSettings> demoOptions,
        ILogger<AdminSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _adminSettings = adminOptions.Value;
        _demoSettings = demoOptions.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await SeedUserAsync(dbContext, hasher, _adminSettings.Email, _adminSettings.Password, Role.Admin, cancellationToken);
        await SeedUserAsync(dbContext, hasher, _demoSettings.Email, _demoSettings.Password, Role.User, cancellationToken);
    }

    private async Task SeedUserAsync(
        ApplicationDbContext dbContext,
        IPasswordHasher hasher,
        string email,
        string password,
        Role role,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var exists = await dbContext.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (exists)
            return;

        dbContext.Users.Add(new User(normalizedEmail, hasher.Hash(password), role));
        await dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Role} user '{Email}'.", role, normalizedEmail);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
