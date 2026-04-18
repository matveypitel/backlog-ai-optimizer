using BacklogOptimizer.Application.Embeddings;
using BacklogOptimizer.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using Pgvector.EntityFrameworkCore;

namespace BacklogOptimizer.Infrastructure.Embeddings;

internal sealed class SimilaritySearchService : ISimilaritySearchService
{
    private readonly ApplicationDbContext _dbContext;

    public SimilaritySearchService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SimilarityMatch>> FindSimilarPagesAsync(
        string jiraKey,
        int topN,
        CancellationToken cancellationToken = default)
    {
        var issue = await _dbContext.JiraIssues
            .Include(j => j.Embedding)
            .FirstOrDefaultAsync(j => j.JiraKey == jiraKey, cancellationToken)
            ?? throw new KeyNotFoundException($"Jira issue '{jiraKey}' not found.");

        if (issue.Embedding is null)
            throw new InvalidOperationException($"Jira issue '{jiraKey}' has no embedding. Run sync first.");

        var issueVector = issue.Embedding.Vector;

        var matches = await _dbContext.ScrapedPageEmbeddings
            .Include(e => e.ScrapedPage)
            .Select(e => new
            {
                e.ScrapedPageId,
                e.ScrapedPage.Url,
                e.ScrapedPage.Title,
                Distance = e.Vector.CosineDistance(issueVector)
            })
            .OrderBy(x => x.Distance)
            .Take(topN)
            .Select(x => new SimilarityMatch(x.ScrapedPageId, x.Url, x.Title, 1.0 - (double)x.Distance))
            .ToListAsync(cancellationToken);

        return matches.AsReadOnly();
    }
}
