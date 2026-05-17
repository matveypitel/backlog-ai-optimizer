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

internal sealed class ReprioritizationAnalyzer
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
    private readonly ILogger<ReprioritizationAnalyzer> _logger;

    public ReprioritizationAnalyzer(
        ILanguageModelClient chatClient,
        ApplicationDbContext dbContext,
        IPromptService promptService,
        IOptions<OpenAiSettings> options,
        ILogger<ReprioritizationAnalyzer> logger)
    {
        _chatClient = chatClient;
        _dbContext = dbContext;
        _promptService = promptService;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<Result<ReprioritizationResult>> AnalyzeAsync(CancellationToken cancellationToken = default)
    {
        var completedStatuses = _settings.CompletedStatuses;

        var issues = await _dbContext.JiraIssues
            .Where(j => !completedStatuses.Contains(j.Status))
            .Include(j => j.Embedding)
            .Where(j => j.Embedding != null)
            .ToListAsync(cancellationToken);

        var featureEmbeddings = await _dbContext.CompetitorFeatureEmbeddings
            .Include(e => e.CompetitorFeature)
                .ThenInclude(f => f.ScrapedPage)
            .ToListAsync(cancellationToken);

        var existing = await _dbContext.ReprioritizationSuggestions.ToListAsync(cancellationToken);
        var existingByKey = existing.ToDictionary(s => s.JiraKey, s => s, StringComparer.OrdinalIgnoreCase);

        var issuesWithFeatures = new List<(JiraIssue Issue, List<(CompetitorFeature Feature, double Score)> Features)>();
        var skippedCount = 0;

        foreach (var issue in issues)
        {
            var topFeatures = featureEmbeddings
                .Select(e => (e.CompetitorFeature, Score: CosineSimilarity(e.Vector, issue.Embedding!.Vector)))
                .Where(x => x.Score > _settings.ReprioritizationSimilarityThreshold)
                .OrderByDescending(x => x.Score)
                .Take(_settings.ReprioritizationSimilarFeaturesTopN)
                .ToList();

            if (topFeatures.Count == 0)
            {
                skippedCount++;
                continue;
            }

            issuesWithFeatures.Add((issue, topFeatures));
        }

        var systemPrompt = await _promptService.BuildSystemPromptAsync(PromptType.Reprioritization, cancellationToken);

        var batchSize = Math.Max(1, _settings.ReprioritizationBatchSize);
        var errors = new List<string>();
        var analyzedCount = 0;

        for (var offset = 0; offset < issuesWithFeatures.Count; offset += batchSize)
        {
            var batch = issuesWithFeatures.Skip(offset).Take(batchSize).ToList();
            var batchNumber = offset / batchSize + 1;
            var batchCount = (issuesWithFeatures.Count + batchSize - 1) / batchSize;

            _logger.LogInformation(
                "Reprioritization batch {BatchNumber}/{BatchCount} ({BatchSize} issues)",
                batchNumber, batchCount, batch.Count);

            var userPrompt = BuildUserPrompt(batch);

            string json;
            try
            {
                json = await _chatClient.CompleteAsync(_settings.CompletionModel, systemPrompt, userPrompt, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "LLM call failed for reprioritization batch {BatchNumber}", batchNumber);
                errors.Add($"batch {batchNumber}: {ex.Message}");
                continue;
            }

            ReprioritizationLlmResponse response;
            try
            {
                response = JsonSerializer.Deserialize<ReprioritizationLlmResponse>(json, ReadOptions)
                    ?? throw new InvalidOperationException("Null LLM response for reprioritization");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to deserialize reprioritization LLM response for batch {BatchNumber}", batchNumber);
                errors.Add($"batch {batchNumber}: {ex.Message}");
                continue;
            }

            var issueLookup = batch.ToDictionary(b => b.Issue.JiraKey, b => b.Issue, StringComparer.OrdinalIgnoreCase);

            foreach (var item in response.Items)
            {
                if (!issueLookup.TryGetValue(item.JiraKey, out var issue))
                {
                    _logger.LogWarning("LLM returned unknown Jira key {JiraKey} for batch {BatchNumber}", item.JiraKey, batchNumber);
                    continue;
                }

                if (item.SuggestedPriority is null)
                {
                    skippedCount++;
                    continue;
                }

                var currentPriority = issue.Priority ?? "None";

                if (existingByKey.TryGetValue(issue.JiraKey, out var current))
                {
                    if (current.CurrentPriority == item.SuggestedPriority)
                        continue;

                    current.Update(
                        currentPriority,
                        item.SuggestedPriority,
                        item.Reasoning,
                        item.CompetitorEvidence,
                        item.ConfidenceScore);
                }
                else
                {
                    var added = new ReprioritizationSuggestion(
                        issue.JiraKey,
                        currentPriority,
                        item.SuggestedPriority,
                        item.Reasoning,
                        item.CompetitorEvidence,
                        item.ConfidenceScore);
                    _dbContext.ReprioritizationSuggestions.Add(added);
                    existingByKey[issue.JiraKey] = added;
                }

                analyzedCount++;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ReprioritizationResult(analyzedCount, skippedCount, errors.AsReadOnly());
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

    private static string BuildUserPrompt(
        IEnumerable<(JiraIssue Issue, List<(CompetitorFeature Feature, double Score)> Features)> batch)
    {
        var sb = new StringBuilder();

        foreach (var (issue, features) in batch)
        {
            sb.AppendLine($"## Backlog Item {issue.JiraKey}");
            sb.AppendLine($"Key: {issue.JiraKey}");
            sb.AppendLine($"Type: {issue.IssueType}");
            sb.AppendLine($"Summary: {issue.Summary}");
            sb.AppendLine($"Current Priority: {issue.Priority ?? "None"}");
            if (!string.IsNullOrWhiteSpace(issue.Description))
                sb.AppendLine($"Description: {issue.Description[..Math.Min(500, issue.Description.Length)]}");

            sb.AppendLine();
            sb.AppendLine("### Similar Competitor Features");

            foreach (var (feature, score) in features)
            {
                sb.Append("- ");
                if (!string.IsNullOrWhiteSpace(feature.Category))
                    sb.Append('[').Append(feature.Category).Append("] ");
                sb.Append(feature.Name).Append(" (similarity: ").Append(score.ToString("F2")).AppendLine(")");
                sb.AppendLine($"  Description: {feature.Description}");
                if (!string.IsNullOrWhiteSpace(feature.Differentiators))
                    sb.AppendLine($"  Differentiators: {feature.Differentiators}");
                if (!string.IsNullOrWhiteSpace(feature.TargetAudience))
                    sb.AppendLine($"  Target audience: {feature.TargetAudience}");
                AppendJsonArray(sb, "Key benefits", feature.KeyBenefits);
                AppendJsonArray(sb, "Use cases", feature.UseCases);
                sb.AppendLine($"  Source: {feature.ScrapedPage.Url}");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static void AppendJsonArray(StringBuilder sb, string label, string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return;

        try
        {
            var items = JsonSerializer.Deserialize<string[]>(json);
            if (items is null || items.Length == 0)
                return;

            sb.Append("  ").Append(label).Append(": ").AppendLine(string.Join("; ", items));
        }
        catch (JsonException)
        {
            // ignore malformed legacy data
        }
    }
}
