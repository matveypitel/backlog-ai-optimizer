using System.Text;
using System.Text.Json;

using BacklogOptimizer.Application.Embeddings;
using BacklogOptimizer.Core.Common;
using BacklogOptimizer.Core.Entities;
using BacklogOptimizer.Infrastructure.Persistence;

using Pgvector;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BacklogOptimizer.Infrastructure.Embeddings;

internal sealed class EmbeddingSyncService : IEmbeddingSyncService
{
    private readonly OpenAiEmbeddingClient _client;
    private readonly ApplicationDbContext _dbContext;
    private readonly OpenAiSettings _settings;
    private readonly ILogger<EmbeddingSyncService> _logger;

    public EmbeddingSyncService(
        OpenAiEmbeddingClient client,
        ApplicationDbContext dbContext,
        IOptions<OpenAiSettings> settings,
        ILogger<EmbeddingSyncService> logger)
    {
        _client = client;
        _dbContext = dbContext;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<Result<EmbeddingSyncResult>> SyncJiraEmbeddingsAsync(CancellationToken cancellationToken = default)
    {
        var completedStatuses = _settings.CompletedStatuses;
        var model = _settings.EmbeddingModel;

        var issues = await _dbContext.JiraIssues
            .Where(j => !completedStatuses.Contains(j.Status))
            .Include(j => j.Embedding)
            .ToListAsync(cancellationToken);

        var embeddedCount = 0;
        var skippedCount = 0;
        var errors = new List<string>();

        foreach (var issue in issues)
        {
            if (issue.Embedding is not null && issue.Embedding.EmbeddedAt >= issue.JiraUpdatedAt)
            {
                skippedCount++;
                continue;
            }

            try
            {
                var text = $"{issue.Summary}\n{issue.Description ?? string.Empty}";
                var floats = await _client.GetEmbeddingAsync(text, model, cancellationToken);
                var vector = new Vector(floats);

                if (issue.Embedding is null)
                    _dbContext.JiraIssueEmbeddings.Add(new JiraIssueEmbedding(issue.Id, model, vector));
                else
                    issue.Embedding.Update(model, vector);

                await _dbContext.SaveChangesAsync(cancellationToken);
                embeddedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to embed Jira issue {Key}", issue.JiraKey);
                errors.Add($"{issue.JiraKey}: {ex.Message}");
                _dbContext.ChangeTracker.Clear();
            }
        }

        return new EmbeddingSyncResult(embeddedCount, skippedCount, errors.AsReadOnly());
    }

    public async Task<Result<EmbeddingSyncResult>> SyncFeatureEmbeddingsAsync(CancellationToken cancellationToken = default)
    {
        var model = _settings.EmbeddingModel;

        var features = await _dbContext.CompetitorFeatures
            .Include(f => f.Embedding)
            .ToListAsync(cancellationToken);

        var embeddedCount = 0;
        var skippedCount = 0;
        var errors = new List<string>();

        foreach (var feature in features)
        {
            if (feature.Embedding is not null && feature.Embedding.EmbeddedAt >= feature.ExtractedAt)
            {
                skippedCount++;
                continue;
            }

            try
            {
                var text = BuildFeatureEmbeddingText(feature);

                var floats = await _client.GetEmbeddingAsync(text, model, cancellationToken);
                var vector = new Vector(floats);

                if (feature.Embedding is null)
                    _dbContext.CompetitorFeatureEmbeddings.Add(new CompetitorFeatureEmbedding(feature.Id, model, vector));
                else
                    feature.Embedding.Update(model, vector);

                await _dbContext.SaveChangesAsync(cancellationToken);
                embeddedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to embed competitor feature {Name}", feature.Name);
                errors.Add($"{feature.Name}: {ex.Message}");
                _dbContext.ChangeTracker.Clear();
            }
        }

        return new EmbeddingSyncResult(embeddedCount, skippedCount, errors.AsReadOnly());
    }

    private static string BuildFeatureEmbeddingText(CompetitorFeature feature)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(feature.Category))
            sb.Append('[').Append(feature.Category).Append("] ");
        sb.AppendLine(feature.Name);
        sb.AppendLine(feature.Description);

        AppendJsonArray(sb, "Benefits", feature.KeyBenefits);
        AppendJsonArray(sb, "Use cases", feature.UseCases);

        if (!string.IsNullOrWhiteSpace(feature.Differentiators))
            sb.Append("Differentiators: ").AppendLine(feature.Differentiators);

        if (!string.IsNullOrWhiteSpace(feature.TargetAudience))
            sb.Append("Audience: ").AppendLine(feature.TargetAudience);

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

            sb.Append(label).Append(": ").AppendLine(string.Join("; ", items));
        }
        catch (JsonException)
        {
        }
    }
}
