using System.Net.Http.Headers;
using System.Text;

using BacklogOptimizer.Application.Analysis;
using BacklogOptimizer.Application.Auth;
using BacklogOptimizer.Application.Embeddings;
using BacklogOptimizer.Application.Jira;
using BacklogOptimizer.Application.Scraping;
using BacklogOptimizer.Application.Settings;
using BacklogOptimizer.Infrastructure.Analysis;
using BacklogOptimizer.Infrastructure.Auth;
using BacklogOptimizer.Infrastructure.BackgroundServices;
using BacklogOptimizer.Infrastructure.Embeddings;
using BacklogOptimizer.Infrastructure.Jira;
using BacklogOptimizer.Application.Prompts;
using BacklogOptimizer.Infrastructure.Persistence;
using BacklogOptimizer.Infrastructure.Prompts;
using BacklogOptimizer.Infrastructure.Scraping;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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
        services.Configure<AnalysisWorkerSettings>(configuration.GetSection(AnalysisWorkerSettings.SectionName));

        services.AddScoped<IScrapingService, ScrapingJobService>();
        services.AddHostedService<ScrapingWorker>();
        services.AddHostedService<FeatureSuggestionWorker>();
        services.AddHostedService<ReprioritizationWorker>();

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

        services.AddHttpClient<OpenAiChatClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.openai.com");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiSettings.ApiKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        services.AddScoped<IReprioritizationService, ReprioritizationJobService>();
        services.AddScoped<IFeatureSuggestionService, FeatureSuggestionJobService>();
        services.AddScoped<ReprioritizationAnalyzer>();
        services.AddScoped<FeatureSuggestionAnalyzer>();
        services.AddScoped<Scraping.FeatureExtractor>();

        services.AddSingleton<PromptResources>();
        services.AddScoped<IPromptService, PromptService>();
        services.AddHostedService<PromptSeeder>();

        AddAuth(services, configuration);

        return services;
    }

    private static void AddAuth(IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException($"'{JwtSettings.SectionName}' configuration section is missing.");

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<AdminSeedSettings>(configuration.GetSection(AdminSeedSettings.SectionName));

        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddHostedService<AdminSeeder>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorization();
    }
}