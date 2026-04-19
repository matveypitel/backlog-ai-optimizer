using BacklogOptimizer.Core.Common;

namespace BacklogOptimizer.Application.Analysis;

public interface IReprioritizationService
{
    Task<Result> EnqueueAnalysisAsync(CancellationToken cancellationToken = default);
}
