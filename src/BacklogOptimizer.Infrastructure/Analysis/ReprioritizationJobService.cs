using BacklogOptimizer.Application.Analysis;
using BacklogOptimizer.Core.Common;
using BacklogOptimizer.Core.Entities;
using BacklogOptimizer.Infrastructure.Persistence;

namespace BacklogOptimizer.Infrastructure.Analysis;

internal sealed class ReprioritizationJobService : IReprioritizationService
{
    private readonly ApplicationDbContext _dbContext;

    public ReprioritizationJobService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> EnqueueAnalysisAsync(CancellationToken cancellationToken = default)
    {
        var job = new ReprioritizationJob();
        await _dbContext.ReprioritizationJobs.AddAsync(job, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
