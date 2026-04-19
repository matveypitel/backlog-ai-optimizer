using System.Text;
using System.Text.Json;

using BacklogOptimizer.Application.Analysis;
using BacklogOptimizer.Application.Embeddings;
using BacklogOptimizer.Core.Entities;
using BacklogOptimizer.Infrastructure.Analysis.Dto;
using BacklogOptimizer.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BacklogOptimizer.Infrastructure.Analysis;

internal sealed class ReprioritizationService : IReprioritizationService
{
    private const string SystemPrompt =
        """
        You are a product management expert analyzing competitive intelligence.
        Given a Jira backlog item and competitor feature pages, evaluate whether the current priority needs adjustment.
        Consider: market pressure indicated by competitor activity, feature parity gaps, and user impact signals.
        If the current priority is already correct and no change is needed, return null from response.
        Only suggest a priority change when there is a clear, concrete reason based on competitor evidence.
        Respond with a JSON object matching exactly this schema:
        {"suggested_priority": "string (Highest|High|Medium|Low|Lowest)", "reasoning": "string", "competitor_evidence": "string (URL + quote)", "confidence_score": number (0.0-1.0)}
        """;

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    private readonly OpenAiChatClient _chatClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly OpenAiSettings _settings;
    private readonly ILogger<ReprioritizationService> _logger;

    public ReprioritizationService(
        OpenAiChatClient chatClient,
        ApplicationDbContext dbContext,
        IOptions<OpenAiSettings> options,
        ILogger<ReprioritizationService> logger)
    {
        _chatClient = chatClient;
        _dbContext = dbContext;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<ReprioritizationResult> AnalyzeAsync(CancellationToken cancellationToken = default)
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

        var suggestions = new List<ReprioritizationSuggestion>();
        var errors = new List<string>();
        var skippedCount = 0;

        foreach (var issue in issues)
        {
            try
            {
                var topPages = pageEmbeddings
                    .Select(e => new
                    {
                        e.ScrapedPage,
                        Score = CosineSimilarity(e.Vector, issue.Embedding!.Vector)
                    })
                    .Where(x => x.Score > 0.3)
                    .OrderByDescending(x => x.Score)
                    .Take(_settings.ReprioritizationSimilarPagesTopN)
                    .ToList();

                if (topPages.Count == 0)
                {
                    skippedCount++;
                    continue;
                }

                var userPrompt = BuildUserPrompt(issue, topPages.Select(p => (p.ScrapedPage, p.Score)));
                var json = await _chatClient.CompleteAsync(_settings.CompletionModel, SystemPrompt, userPrompt, cancellationToken);

                var llmResponse = JsonSerializer.Deserialize<ReprioritizationLlmResponse>(json, ReadOptions)
                    ?? throw new InvalidOperationException("Null LLM response for reprioritization");

                if (llmResponse.SuggestedPriority is null)
                {
                    skippedCount++;
                    continue;
                }

                suggestions.Add(new ReprioritizationSuggestion(
                    issue.JiraKey,
                    issue.Priority ?? "None",
                    llmResponse.SuggestedPriority,
                    llmResponse.Reasoning,
                    llmResponse.CompetitorEvidence,
                    llmResponse.ConfidenceScore));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to analyze reprioritization for issue {JiraKey}", issue.JiraKey);
                errors.Add($"{issue.JiraKey}: {ex.Message}");
            }
        }

        await _dbContext.ReprioritizationSuggestions.ExecuteDeleteAsync(cancellationToken);
        _dbContext.ReprioritizationSuggestions.AddRange(suggestions);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ReprioritizationResult(suggestions.Count, skippedCount, errors.AsReadOnly());
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
        JiraIssue issue,
        IEnumerable<(ScrapedPage Page, double Score)> similarPages)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Backlog Item");
        sb.AppendLine($"Key: {issue.JiraKey}");
        sb.AppendLine($"Type: {issue.IssueType}");
        sb.AppendLine($"Summary: {issue.Summary}");
        sb.AppendLine($"Current Priority: {issue.Priority ?? "None"}");
        if (!string.IsNullOrWhiteSpace(issue.Description))
            sb.AppendLine($"Description: {issue.Description[..Math.Min(500, issue.Description.Length)]}");

        sb.AppendLine();
        sb.AppendLine("## Competitor Pages (most similar)");

        foreach (var (page, score) in similarPages)
        {
            sb.AppendLine($"URL: {page.Url} (similarity: {score:F2})");
            if (!string.IsNullOrWhiteSpace(page.Title))
                sb.AppendLine($"Title: {page.Title}");
            if (!string.IsNullOrWhiteSpace(page.ExtractedContent))
                sb.AppendLine($"Content: {page.ExtractedContent[..Math.Min(1000, page.ExtractedContent.Length)]}");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
