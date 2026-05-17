using System.Text;
using System.Text.Json;

using BacklogOptimizer.Application.Analysis;
using BacklogOptimizer.Application.Embeddings;
using BacklogOptimizer.Application.Prompts;
using BacklogOptimizer.Core.Common;
using BacklogOptimizer.Core.Entities;
using BacklogOptimizer.Infrastructure.Analysis.Dto;
using BacklogOptimizer.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BacklogOptimizer.Infrastructure.Analysis;

internal sealed class FeatureSuggestionAnalyzer
{
    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    private readonly ILanguageModelClient _chatClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly IPromptService _promptService;
    private readonly OpenAiSettings _settings;
    private readonly ILogger<FeatureSuggestionAnalyzer> _logger;

    public FeatureSuggestionAnalyzer(
        ILanguageModelClient chatClient,
        ApplicationDbContext dbContext,
        IPromptService promptService,
        IOptions<OpenAiSettings> options,
        ILogger<FeatureSuggestionAnalyzer> logger)
    {
        _chatClient = chatClient;
        _dbContext = dbContext;
        _promptService = promptService;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<Result<FeatureSuggestionResult>> AnalyzeAsync(CancellationToken cancellationToken = default)
    {
        var completedStatuses = _settings.CompletedStatuses;

        var issueEmbeddings = await _dbContext.JiraIssueEmbeddings
            .Include(e => e.JiraIssue)
            .Where(e => !completedStatuses.Contains(e.JiraIssue.Status))
            .ToListAsync(cancellationToken);

        var featureEmbeddings = await _dbContext.CompetitorFeatureEmbeddings
            .Include(e => e.CompetitorFeature)
                .ThenInclude(f => f.ScrapedPage)
            .ToListAsync(cancellationToken);

        var gapFeatures = featureEmbeddings
            .Where(fe => issueEmbeddings.Count == 0 || issueEmbeddings
                .All(ie => CosineSimilarity(fe.Vector, ie.Vector) < _settings.CoverageGapThreshold))
            .Select(fe => fe.CompetitorFeature)
            .ToList();

        if (gapFeatures.Count == 0)
            return new FeatureSuggestionResult(0, 0, []);

        var backlogSummary = issueEmbeddings
            .Select(ie => $"[{ie.JiraIssue.JiraKey}] {ie.JiraIssue.Summary}")
            .ToList();

        var existing = await _dbContext.FeatureSuggestions.ToListAsync(cancellationToken);
        var existingByTitle = existing.ToDictionary(f => f.Title, f => f, StringComparer.OrdinalIgnoreCase);

        var systemPrompt = await _promptService.BuildSystemPromptAsync(PromptType.FeatureSuggestion, cancellationToken);

        var batchSize = Math.Max(1, _settings.FeatureSuggestionBatchSize);
        var errors = new List<string>();
        var suggestionsGenerated = 0;

        for (var offset = 0; offset < gapFeatures.Count; offset += batchSize)
        {
            var batch = gapFeatures.Skip(offset).Take(batchSize).ToList();
            var batchNumber = offset / batchSize + 1;
            var batchCount = (gapFeatures.Count + batchSize - 1) / batchSize;

            _logger.LogInformation(
                "Feature suggestion batch {BatchNumber}/{BatchCount} ({BatchSize} features)",
                batchNumber, batchCount, batch.Count);

            var userPrompt = BuildUserPrompt(backlogSummary, batch);

            string json;
            try
            {
                json = await _chatClient.CompleteAsync(_settings.CompletionModel, systemPrompt, userPrompt, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "LLM call failed for feature suggestion batch {BatchNumber}", batchNumber);
                errors.Add($"batch {batchNumber}: {ex.Message}");
                continue;
            }

            FeatureSuggestionsWrapper wrapper;
            try
            {
                wrapper = JsonSerializer.Deserialize<FeatureSuggestionsWrapper>(json, ReadOptions)
                    ?? throw new InvalidOperationException("Null LLM response for feature suggestions");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to deserialize feature suggestion LLM response for batch {BatchNumber}", batchNumber);
                errors.Add($"batch {batchNumber}: {ex.Message}");
                continue;
            }

            foreach (var item in wrapper.Suggestions)
            {
                var tags = JsonSerializer.Serialize(item.Tags ?? []);
                var userStories = JsonSerializer.Serialize(item.UserStories ?? []);
                var acceptanceCriteria = JsonSerializer.Serialize(item.AcceptanceCriteria ?? []);

                if (existingByTitle.TryGetValue(item.Title, out var current))
                {
                    current.Update(
                        item.Description,
                        item.IssueType,
                        item.SuggestedPriority,
                        tags,
                        item.Reasoning,
                        item.CompetitorEvidence,
                        item.BusinessValue ?? string.Empty,
                        item.EstimatedImpact ?? string.Empty,
                        userStories,
                        acceptanceCriteria);
                }
                else
                {
                    var added = new FeatureSuggestion(
                        item.Title,
                        item.Description,
                        item.IssueType,
                        item.SuggestedPriority,
                        tags,
                        item.Reasoning,
                        item.CompetitorEvidence,
                        item.BusinessValue ?? string.Empty,
                        item.EstimatedImpact ?? string.Empty,
                        userStories,
                        acceptanceCriteria);
                    _dbContext.FeatureSuggestions.Add(added);
                    existingByTitle[item.Title] = added;
                }

                suggestionsGenerated++;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new FeatureSuggestionResult(gapFeatures.Count, suggestionsGenerated, errors);
    }

    private static double CosineSimilarity(Pgvector.Vector a, Pgvector.Vector b)
    {
        var aArr = a.ToArray();
        var bArr = b.ToArray();
        double dot = 0, magA = 0, magB = 0;
        for (var i = 0; i < aArr.Length; i++)
        {
            dot += aArr[i] * bArr[i];
            magA += aArr[i] * aArr[i];
            magB += bArr[i] * bArr[i];
        }
        return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
    }

    private static string BuildUserPrompt(IEnumerable<string> backlogSummary, IEnumerable<CompetitorFeature> gapFeatures)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Current Backlog (active items)");
        foreach (var item in backlogSummary)
            sb.AppendLine(item);

        sb.AppendLine();
        sb.AppendLine("## Competitor Features Not Covered by Backlog");

        foreach (var feature in gapFeatures)
        {
            sb.Append("- ");
            if (!string.IsNullOrWhiteSpace(feature.Category))
                sb.Append('[').Append(feature.Category).Append("] ");
            sb.AppendLine(feature.Name);
            sb.AppendLine($"  Description: {feature.Description}");
            sb.AppendLine($"  Source: {feature.ScrapedPage.Url}");
        }

        return sb.ToString();
    }
}
