using BacklogOptimizer.Core.Common;

namespace BacklogOptimizer.Application.Analysis;

public interface IReprioritizationService
{
    Task<Result<ReprioritizationResult>> AnalyzeAsync(CancellationToken cancellationToken = default);
}
