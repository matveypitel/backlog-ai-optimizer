using BacklogOptimizer.Application.Scraping;
using BacklogOptimizer.Application.Settings;
using BacklogOptimizer.Infrastructure.BackgroundServices;
using BacklogOptimizer.Infrastructure.Persistence;
using BacklogOptimizer.Infrastructure.Scraping;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BacklogOptimizer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.Configure<ScrapingSettings>(configuration.GetSection(ScrapingSettings.SectionName));
        services.Configure<ScrapingWorkerSettings>(configuration.GetSection(ScrapingWorkerSettings.SectionName));

        services.AddScoped<IScrapingService, ScrapingJobService>();
        services.AddHostedService<ScrapingWorker>();

        return services;
    }
}