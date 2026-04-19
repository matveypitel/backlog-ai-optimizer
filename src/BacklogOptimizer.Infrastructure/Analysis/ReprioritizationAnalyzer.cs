using System.Text;
using System.Text.Json;

using BacklogOptimizer.Application.Analysis;
using BacklogOptimizer.Application.Embeddings;
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
    private const string SystemPrompt =
        """
        You are a product management expert analyzing competitive intelligence.
        You will receive a list of Jira backlog items, each with its most similar competitor feature pages.
        For each item, evaluate whether the current priority needs adjustment.
        Consider: market pressure indicated by competitor activity, feature parity gaps, and user impact signals.
        Only suggest a priority change when there is a clear, concrete reason based on competitor evidence.
        If the current priority is already correct and no change is needed, return "suggested_priority": null for that item.
        Respond with a JSON object matching exactly this schema:
        {"items": [{"jira_key": "string", "suggested_priority": "string (Highest|High|Medium|Low|Lowest) or null", "reasoning": "string", "competitor_evidence": "string (URL + quote)", "confidence_score": number (0.0-1.0)}]}
        Include exactly one entry per input item, keyed by its jira_key.
        """;

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    private readonly OpenAiChatClient _chatClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly OpenAiSettings _settings;
    private readonly ILogger<ReprioritizationAnalyzer> _logger;

    public ReprioritizationAnalyzer(
        OpenAiChatClient chatClient,
        ApplicationDbContext dbContext,
        IOptions<OpenAiSettings> options,
        ILogger<ReprioritizationAnalyzer> logger)
    {
        _chatClient = chatClient;
        _dbContext = dbContext;
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

        var pageEmbeddings = await _dbContext.ScrapedPageEmbeddings
            .Include(e => e.ScrapedPage)
            .ToListAsync(cancellationToken);

        var existing = await _dbContext.ReprioritizationSuggestions.ToListAsync(cancellationToken);
        var existingByKey = existing.ToDictionary(s => s.JiraKey, s => s, StringComparer.OrdinalIgnoreCase);

        var issuesWithPages = new List<(JiraIssue Issue, List<(ScrapedPage Page, double Score)> Pages)>();
        var skippedCount = 0;

        foreach (var issue in issues)
        {
            var topPages = pageEmbeddings
                .Select(e => (e.ScrapedPage, Score: CosineSimilarity(e.Vector, issue.Embedding!.Vector)))
                .Where(x => x.Score > _settings.ReprioritizationSimilarityThreshold)
                .OrderByDescending(x => x.Score)
                .Take(_settings.ReprioritizationSimilarPagesTopN)
                .ToList();

            if (topPages.Count == 0)
            {
                skippedCount++;
                continue;
            }

            issuesWithPages.Add((issue, topPages));
        }

        var batchSize = Math.Max(1, _settings.ReprioritizationBatchSize);
        var errors = new List<string>();
        var analyzedCount = 0;

        for (var offset = 0; offset < issuesWithPages.Count; offset += batchSize)
        {
            var batch = issuesWithPages.Skip(offset).Take(batchSize).ToList();
            var batchNumber = offset / batchSize + 1;
            var batchCount = (issuesWithPages.Count + batchSize - 1) / batchSize;

            _logger.LogInformation(
                "Reprioritization batch {BatchNumber}/{BatchCount} ({BatchSize} issues)",
                batchNumber, batchCount, batch.Count);

            var userPrompt = BuildUserPrompt(batch);

            string json;
            try
            {
                json = await _chatClient.CompleteAsync(_settings.CompletionModel, SystemPrompt, userPrompt, cancellationToken);
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
                    {
                        // No change from current priority, so skip updating
                        continue;
                    }

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
        IEnumerable<(JiraIssue Issue, List<(ScrapedPage Page, double Score)> Pages)> batch)
    {
        var sb = new StringBuilder();

        foreach (var (issue, pages) in batch)
        {
            sb.AppendLine($"## Backlog Item {issue.JiraKey}");
            sb.AppendLine($"Key: {issue.JiraKey}");
            sb.AppendLine($"Type: {issue.IssueType}");
            sb.AppendLine($"Summary: {issue.Summary}");
            sb.AppendLine($"Current Priority: {issue.Priority ?? "None"}");
            if (!string.IsNullOrWhiteSpace(issue.Description))
                sb.AppendLine($"Description: {issue.Description[..Math.Min(500, issue.Description.Length)]}");

            sb.AppendLine();
            sb.AppendLine("### Competitor Pages (most similar)");

            foreach (var (page, score) in pages)
            {
                sb.AppendLine($"URL: {page.Url} (similarity: {score:F2})");
                if (!string.IsNullOrWhiteSpace(page.Title))
                    sb.AppendLine($"Title: {page.Title}");
                if (!string.IsNullOrWhiteSpace(page.ExtractedContent))
                    sb.AppendLine($"Content: {page.ExtractedContent[..Math.Min(1000, page.ExtractedContent.Length)]}");
                sb.AppendLine();
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}
