using System.Text.Json;

using BacklogOptimizer.Application.Embeddings;
using BacklogOptimizer.Core.Entities;
using BacklogOptimizer.Infrastructure.Analysis;
using BacklogOptimizer.Infrastructure.Analysis.Dto;
using BacklogOptimizer.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BacklogOptimizer.Infrastructure.Scraping;

internal sealed class FeatureExtractor
{
    private const string SystemPrompt =
        """
        You are a product analyst reading a competitor's web page.
        Extract the distinct, user-facing product features described on the page.
        Each feature must be concrete and non-overlapping. Skip marketing fluff, pricing, and company info.
        Respond with a JSON object: {"features": [{"name": "string (short, 3-8 words)", "description": "string (1-3 sentences, what the feature does for the user)", "category": "string or null (e.g. Analytics, Collaboration, Integrations)"}]}.
        If the page has no product features, return {"features": []}.
        """;

    private const int MaxContentChars = 8000;

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    private readonly OpenAiChatClient _chatClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly OpenAiSettings _settings;
    private readonly ILogger<FeatureExtractor> _logger;

    public FeatureExtractor(
        OpenAiChatClient chatClient,
        ApplicationDbContext dbContext,
        IOptions<OpenAiSettings> settings,
        ILogger<FeatureExtractor> logger)
    {
        _chatClient = chatClient;
        _dbContext = dbContext;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task ExtractAsync(Guid scrapedPageId, CancellationToken cancellationToken)
    {
        var page = await _dbContext.ScrapedPages
            .Include(p => p.Features)
            .FirstOrDefaultAsync(p => p.Id == scrapedPageId, cancellationToken);

        if (page is null)
            return;

        var content = page.ExtractedContent;
        if (string.IsNullOrWhiteSpace(content))
        {
            _logger.LogInformation("Skipping feature extraction for {Url}: no extracted content", page.Url);
            return;
        }

        if (content.Length > MaxContentChars)
            content = content[..MaxContentChars];

        var userPrompt = $"URL: {page.Url}\nTitle: {page.Title ?? string.Empty}\n\nContent:\n{content}";

        var json = await _chatClient.CompleteAsync(_settings.CompletionModel, SystemPrompt, userPrompt, cancellationToken);

        var wrapper = JsonSerializer.Deserialize<CompetitorFeaturesWrapper>(json, ReadOptions)
            ?? throw new InvalidOperationException("Null LLM response for feature extraction");

        // Replace strategy: drop existing features (and their embeddings via cascade), add fresh set.
        if (page.Features.Count > 0)
            _dbContext.CompetitorFeatures.RemoveRange(page.Features);

        foreach (var item in wrapper.Features)
        {
            if (string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.Description))
                continue;

            _dbContext.CompetitorFeatures.Add(new CompetitorFeature(
                page.Id,
                item.Name.Trim(),
                item.Description.Trim(),
                string.IsNullOrWhiteSpace(item.Category) ? null : item.Category.Trim()));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Extracted {Count} features for {Url}", wrapper.Features.Length, page.Url);
    }
}
