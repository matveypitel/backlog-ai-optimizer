using BacklogOptimizer.Application.Embeddings;
using BacklogOptimizer.Core.Common;
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

    public async Task<Result<IReadOnlyList<SimilarityMatch>>> FindSimilarFeaturesAsync(
        string jiraKey,
        int topN,
        CancellationToken cancellationToken = default)
    {
        var issue = await _dbContext.JiraIssues
            .Include(j => j.Embedding)
            .FirstOrDefaultAsync(j => j.JiraKey == jiraKey, cancellationToken);

        if (issue is null)
            return Errors.NotFound.JiraIssue(jiraKey);

        if (issue.Embedding is null)
            return Errors.NotFound.JiraIssueEmbedding(jiraKey);

        var issueVector = issue.Embedding.Vector;

        var matches = await _dbContext.CompetitorFeatureEmbeddings
            .Include(e => e.CompetitorFeature)
                .ThenInclude(f => f.ScrapedPage)
            .Select(e => new
            {
                e.CompetitorFeatureId,
                e.CompetitorFeature.Name,
                e.CompetitorFeature.Category,
                e.CompetitorFeature.ScrapedPageId,
                e.CompetitorFeature.ScrapedPage.Url,
                Distance = e.Vector.CosineDistance(issueVector)
            })
            .OrderBy(x => x.Distance)
            .Take(topN)
            .Select(x => new SimilarityMatch(x.CompetitorFeatureId, x.Name, x.Category, x.ScrapedPageId, x.Url, 1.0 - x.Distance))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<SimilarityMatch>>.Success(matches.AsReadOnly());
    }
}
