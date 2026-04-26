using BacklogOptimizer.Application.Scraping;
using BacklogOptimizer.Core.Common;
using BacklogOptimizer.Core.Entities;
using BacklogOptimizer.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace BacklogOptimizer.Infrastructure.Scraping;

internal sealed class ScrapingSourceService : IScrapingSourceService
{
    private readonly ApplicationDbContext _dbContext;

    public ScrapingSourceService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ScrapingSourceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ScrapingSources
            .OrderByDescending(s => s.IsActive)
            .ThenBy(s => s.CreatedAt)
            .Select(s => new ScrapingSourceDto(
                s.Id, s.Url, s.Name, s.IsActive, s.LastScrapedAt,
                s.RefreshIntervalHours, s.CreatedAt, s.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<Result<ScrapingSourceDto>> CreateAsync(CreateScrapingSourceCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Url))
            return Errors.Validation.UrlRequired;

        var url = command.Url.Trim();

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            return Errors.Validation.UrlMalformed(url);

        var exists = await _dbContext.ScrapingSources.AnyAsync(s => s.Url == url, cancellationToken);
        if (exists)
            return Errors.ScrapingSource.UrlAlreadyExists(url);

        var source = new ScrapingSource(url, command.Name, command.RefreshIntervalHours);
        await _dbContext.ScrapingSources.AddAsync(source, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(source);
    }

    public async Task<Result<ScrapingSourceDto>> UpdateAsync(Guid id, UpdateScrapingSourceCommand command, CancellationToken cancellationToken = default)
    {
        var source = await _dbContext.ScrapingSources.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (source is null)
            return Errors.ScrapingSource.NotFound(id);

        source.Update(command.Name, command.IsActive, command.RefreshIntervalHours);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(source);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var source = await _dbContext.ScrapingSources.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (source is null)
            return Errors.ScrapingSource.NotFound(id);

        _dbContext.ScrapingSources.Remove(source);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<SyncAllResult>> SyncAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var sources = await _dbContext.ScrapingSources
            .Where(s => s.IsActive)
            .ToListAsync(cancellationToken);

        var enqueued = 0;
        var skipped = 0;

        foreach (var source in sources)
        {
            // Skip if there's already a Pending or Running job for this source — avoids
            // piling up duplicates if the user mashes Sync all repeatedly.
            var inFlight = await _dbContext.ScrapingJobs.AnyAsync(
                j => j.SourceId == source.Id
                    && (j.Status == ScrapingJobStatus.Pending || j.Status == ScrapingJobStatus.Running),
                cancellationToken);

            if (inFlight)
            {
                skipped++;
                continue;
            }

            await _dbContext.ScrapingJobs.AddAsync(new ScrapingJob(source.Url, source.Id), cancellationToken);
            enqueued++;
        }

        if (enqueued > 0)
            await _dbContext.SaveChangesAsync(cancellationToken);

        return new SyncAllResult(enqueued, skipped);
    }

    public async Task<Result> SyncOneAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var source = await _dbContext.ScrapingSources.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (source is null)
            return Errors.ScrapingSource.NotFound(id);

        var inFlight = await _dbContext.ScrapingJobs.AnyAsync(
            j => j.SourceId == source.Id
                && (j.Status == ScrapingJobStatus.Pending || j.Status == ScrapingJobStatus.Running),
            cancellationToken);

        if (inFlight)
            return Result.Success();

        await _dbContext.ScrapingJobs.AddAsync(new ScrapingJob(source.Url, source.Id), cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static ScrapingSourceDto ToDto(ScrapingSource s) => new(
        s.Id, s.Url, s.Name, s.IsActive, s.LastScrapedAt,
        s.RefreshIntervalHours, s.CreatedAt, s.UpdatedAt);
}
