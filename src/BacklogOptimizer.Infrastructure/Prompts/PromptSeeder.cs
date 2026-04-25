using BacklogOptimizer.Core.Entities;
using BacklogOptimizer.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BacklogOptimizer.Infrastructure.Prompts;

internal sealed class PromptSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PromptSeeder> _logger;

    public PromptSeeder(IServiceProvider serviceProvider, ILogger<PromptSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var resources = scope.ServiceProvider.GetRequiredService<PromptResources>();

        var existing = await dbContext.PromptTemplates
            .Select(t => t.Type)
            .ToListAsync(cancellationToken);

        var existingSet = existing.ToHashSet();
        var allTypes = Enum.GetValues<PromptType>();
        var added = 0;

        foreach (var type in allTypes)
        {
            if (existingSet.Contains(type))
                continue;

            dbContext.PromptTemplates.Add(new PromptTemplate(type, resources.GetDefaultInstructions(type)));
            added++;
        }

        if (added > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded {Count} prompt template(s) with default instructions.", added);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
