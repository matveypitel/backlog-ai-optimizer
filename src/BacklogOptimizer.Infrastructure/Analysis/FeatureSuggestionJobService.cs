using BacklogOptimizer.Application.Analysis;
using BacklogOptimizer.Core.Common;
using BacklogOptimizer.Core.Entities;
using BacklogOptimizer.Infrastructure.Persistence;

namespace BacklogOptimizer.Infrastructure.Analysis;

internal sealed class FeatureSuggestionJobService : IFeatureSuggestionService
{
    private readonly ApplicationDbContext _dbContext;

    public FeatureSuggestionJobService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> EnqueueAnalysisAsync(CancellationToken cancellationToken = default)
    {
        var job = new FeatureSuggestionJob();
        await _dbContext.FeatureSuggestionJobs.AddAsync(job, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
