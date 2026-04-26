using BacklogOptimizer.Core.Common;

namespace BacklogOptimizer.Application.Scraping;

public interface IScrapingSourceService
{
    Task<IReadOnlyList<ScrapingSourceDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<ScrapingSourceDto>> CreateAsync(CreateScrapingSourceCommand command, CancellationToken cancellationToken = default);

    Task<Result<ScrapingSourceDto>> UpdateAsync(Guid id, UpdateScrapingSourceCommand command, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<SyncAllResult>> SyncAllActiveAsync(CancellationToken cancellationToken = default);

    Task<Result> SyncOneAsync(Guid id, CancellationToken cancellationToken = default);
}
