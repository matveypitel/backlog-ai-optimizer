using System.Text.Json;

using BacklogOptimizer.Application.Analysis;
using BacklogOptimizer.Application.Embeddings;
using BacklogOptimizer.Application.Prompts;
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
    private const int MaxContentChars = 8000;

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    private readonly ILanguageModelClient _chatClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly IPromptService _promptService;
    private readonly IEmbeddingSyncService _embeddingSyncService;
    private readonly OpenAiSettings _settings;
    private readonly ILogger<FeatureExtractor> _logger;

    public FeatureExtractor(
        ILanguageModelClient chatClient,
        ApplicationDbContext dbContext,
        IPromptService promptService,
        IEmbeddingSyncService embeddingSyncService,
        IOptions<OpenAiSettings> settings,
        ILogger<FeatureExtractor> logger)
    {
        _chatClient = chatClient;
        _dbContext = dbContext;
        _promptService = promptService;
        _embeddingSyncService = embeddingSyncService;
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

        var systemPrompt = await _promptService.BuildSystemPromptAsync(PromptType.FeatureExtraction, cancellationToken);
        var json = await _chatClient.CompleteAsync(_settings.CompletionModel, systemPrompt, userPrompt, cancellationToken);

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
                string.IsNullOrWhiteSpace(item.Category) ? null : item.Category.Trim(),
                JsonSerializer.Serialize(item.KeyBenefits ?? []),
                JsonSerializer.Serialize(item.UseCases ?? []),
                item.Differentiators?.Trim() ?? string.Empty,
                string.IsNullOrWhiteSpace(item.TargetAudience) ? null : item.TargetAudience.Trim()));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Extracted {Count} features for {Url}", wrapper.Features.Length, page.Url);

        try
        {
            var embeddingResult = await _embeddingSyncService.SyncFeatureEmbeddingsAsync(cancellationToken);
            if (embeddingResult.IsSuccess)
            {
                _logger.LogInformation(
                    "Auto-embedded {Embedded} feature(s), skipped {Skipped}",
                    embeddingResult.Value!.EmbeddedCount, embeddingResult.Value.SkippedCount);
            }
            else
            {
                _logger.LogWarning(
                    "Auto-embedding after feature extraction reported error: {Error}",
                    embeddingResult.Error!.Description);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Auto-embedding after feature extraction failed for {Url}", page.Url);
        }
    }
}
