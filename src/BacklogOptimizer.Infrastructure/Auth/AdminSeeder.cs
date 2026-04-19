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
    private readonly AdminSeedSettings _settings;
    private readonly ILogger<AdminSeeder> _logger;

    public AdminSeeder(
        IServiceScopeFactory scopeFactory,
        IOptions<AdminSeedSettings> options,
        ILogger<AdminSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.Email) || string.IsNullOrWhiteSpace(_settings.Password))
        {
            _logger.LogInformation("AdminSeed is not configured; skipping admin seeding.");
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var email = _settings.Email.Trim().ToLowerInvariant();
        var exists = await dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);
        if (exists)
            return;

        dbContext.Users.Add(new User(email, hasher.Hash(_settings.Password), Role.Admin));
        await dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded admin user '{Email}'.", email);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
