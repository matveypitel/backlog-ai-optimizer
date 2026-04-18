using BacklogOptimizer.Application.Embeddings;
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

    public async Task<EmbeddingSyncResult> SyncJiraEmbeddingsAsync(CancellationToken cancellationToken = default)
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

    public async Task<EmbeddingSyncResult> SyncPageEmbeddingsAsync(CancellationToken cancellationToken = default)
    {
        var model = _settings.EmbeddingModel;

        var pages = await _dbContext.ScrapedPages
            .Include(p => p.Embedding)
            .ToListAsync(cancellationToken);

        var embeddedCount = 0;
        var skippedCount = 0;
        var errors = new List<string>();

        foreach (var page in pages)
        {
            if (page.Embedding is not null && page.Embedding.EmbeddedAt >= (page.UpdatedAt ?? page.CreatedAt))
            {
                skippedCount++;
                continue;
            }

            try
            {
                var text = $"{page.Title ?? string.Empty}\n{page.ExtractedContent ?? string.Empty}";
                var floats = await _client.GetEmbeddingAsync(text, model, cancellationToken);
                var vector = new Vector(floats);

                if (page.Embedding is null)
                    _dbContext.ScrapedPageEmbeddings.Add(new ScrapedPageEmbedding(page.Id, model, vector));
                else
                    page.Embedding.Update(model, vector);

                await _dbContext.SaveChangesAsync(cancellationToken);
                embeddedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to embed scraped page {Url}", page.Url);
                errors.Add($"{page.Url}: {ex.Message}");
                _dbContext.ChangeTracker.Clear();
            }
        }

        return new EmbeddingSyncResult(embeddedCount, skippedCount, errors.AsReadOnly());
    }
}
