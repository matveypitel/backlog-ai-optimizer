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

internal sealed class FeatureSuggestionService : IFeatureSuggestionService
{
    private const string SystemPrompt =
        """
        You are a product management expert identifying competitive feature gaps.
        Given the current product backlog and competitor pages NOT covered by the backlog, identify new features that should be added.
        Each suggestion must be a distinct, actionable feature not already in the backlog.
        Respond with a JSON object with one key "suggestions" containing an array. Each item matches:
        {"title": "string", "description": "string", "issue_type": "Story|Task|Epic", "suggested_priority": "Highest|High|Medium|Low|Lowest", "tags": ["string"], "reasoning": "string", "competitor_evidence": "string"}
        """;

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    private readonly OpenAiChatClient _chatClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly OpenAiSettings _settings;
    private readonly ILogger<FeatureSuggestionService> _logger;

    public FeatureSuggestionService(
        OpenAiChatClient chatClient,
        ApplicationDbContext dbContext,
        IOptions<OpenAiSettings> options,
        ILogger<FeatureSuggestionService> logger)
    {
        _chatClient = chatClient;
        _dbContext = dbContext;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<FeatureSuggestionResult> AnalyzeAsync(CancellationToken cancellationToken = default)
    {
        var completedStatuses = _settings.CompletedStatuses;

        var issueEmbeddings = await _dbContext.JiraIssueEmbeddings
            .Include(e => e.JiraIssue)
            .Where(e => !completedStatuses.Contains(e.JiraIssue.Status))
            .ToListAsync(cancellationToken);

        var pageEmbeddings = await _dbContext.ScrapedPageEmbeddings
            .Include(e => e.ScrapedPage)
            .ToListAsync(cancellationToken);

        var gapPages = pageEmbeddings
            .Where(pe => issueEmbeddings.Count == 0 || issueEmbeddings
                .All(ie => CosineSimilarity(pe.Vector, ie.Vector) < _settings.CoverageGapThreshold))
            .Select(pe => pe.ScrapedPage)
            .ToList();

        if (gapPages.Count == 0)
            return new FeatureSuggestionResult(0, 0, []);

        var backlogSummary = issueEmbeddings
            .Select(ie => $"[{ie.JiraIssue.JiraKey}] {ie.JiraIssue.Summary}")
            .Take(50);

        var userPrompt = BuildUserPrompt(backlogSummary, gapPages);

        string json;
        try
        {
            json = await _chatClient.CompleteAsync(_settings.CompletionModel, SystemPrompt, userPrompt, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LLM call failed for feature suggestion analysis");
            return new FeatureSuggestionResult(gapPages.Count, 0, [$"LLM call failed: {ex.Message}"]);
        }

        FeatureSuggestionsWrapper wrapper;
        try
        {
            wrapper = JsonSerializer.Deserialize<FeatureSuggestionsWrapper>(json, ReadOptions)
                ?? throw new InvalidOperationException("Null LLM response for feature suggestions");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize feature suggestion LLM response");
            return new FeatureSuggestionResult(gapPages.Count, 0, [$"Failed to parse LLM response: {ex.Message}"]);
        }

        var suggestions = wrapper.Suggestions
            .Select(item => new FeatureSuggestion(
                item.Title,
                item.Description,
                item.IssueType,
                item.SuggestedPriority,
                JsonSerializer.Serialize(item.Tags),
                item.Reasoning,
                item.CompetitorEvidence))
            .ToList();

        await _dbContext.FeatureSuggestions.ExecuteDeleteAsync(cancellationToken);
        _dbContext.FeatureSuggestions.AddRange(suggestions);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new FeatureSuggestionResult(gapPages.Count, suggestions.Count, []);
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

    private static string BuildUserPrompt(IEnumerable<string> backlogSummary, IEnumerable<ScrapedPage> gapPages)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## Current Backlog (active items)");
        foreach (var item in backlogSummary)
            sb.AppendLine(item);

        sb.AppendLine();
        sb.AppendLine("## Competitor Pages Not Covered by Backlog");

        foreach (var page in gapPages)
        {
            sb.AppendLine($"URL: {page.Url}");
            if (!string.IsNullOrWhiteSpace(page.Title))
                sb.AppendLine($"Title: {page.Title}");
            if (!string.IsNullOrWhiteSpace(page.ExtractedContent))
                sb.AppendLine($"Content: {page.ExtractedContent[..Math.Min(500, page.ExtractedContent.Length)]}");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
