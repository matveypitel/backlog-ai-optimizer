using System.Net.Http.Headers;
using System.Text;

using BacklogOptimizer.Application.Embeddings;
using BacklogOptimizer.Application.Jira;
using BacklogOptimizer.Application.Scraping;
using BacklogOptimizer.Application.Settings;
using BacklogOptimizer.Infrastructure.BackgroundServices;
using BacklogOptimizer.Infrastructure.Embeddings;
using BacklogOptimizer.Infrastructure.Jira;
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
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                o => o.UseVector()));

        services.Configure<ScrapingSettings>(configuration.GetSection(ScrapingSettings.SectionName));
        services.Configure<ScrapingWorkerSettings>(configuration.GetSection(ScrapingWorkerSettings.SectionName));

        services.AddScoped<IScrapingService, ScrapingJobService>();
        services.AddHostedService<ScrapingWorker>();

        var jiraSettings = configuration.GetSection(JiraSettings.SectionName).Get<JiraSettings>()
            ?? throw new InvalidOperationException($"'{JiraSettings.SectionName}' configuration section is missing.");

        services.Configure<JiraSettings>(configuration.GetSection(JiraSettings.SectionName));

        services.AddHttpClient<JiraApiClient>(client =>
        {
            client.BaseAddress = new Uri(jiraSettings.BaseUrl);
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{jiraSettings.Email}:{jiraSettings.ApiToken}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        services.AddScoped<IJiraSyncService, JiraSyncService>();

        var openAiSettings = configuration.GetSection(OpenAiSettings.SectionName).Get<OpenAiSettings>()
            ?? throw new InvalidOperationException($"'{OpenAiSettings.SectionName}' configuration section is missing.");

        services.Configure<OpenAiSettings>(configuration.GetSection(OpenAiSettings.SectionName));

        services.AddHttpClient<OpenAiEmbeddingClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.openai.com");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiSettings.ApiKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        services.AddScoped<IEmbeddingSyncService, EmbeddingSyncService>();
        services.AddScoped<ISimilaritySearchService, SimilaritySearchService>();

        return services;
    }
}