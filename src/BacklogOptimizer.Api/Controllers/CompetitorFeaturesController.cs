using BacklogOptimizer.Api.Models;
using BacklogOptimizer.Infrastructure.Persistence;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BacklogOptimizer.Api.Controllers;

[ApiController]
[Route("api/competitor-features")]
[Authorize]
public sealed class CompetitorFeaturesController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public CompetitorFeaturesController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CompetitorFeatureResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] Guid? pageId = null,
        [FromQuery] int take = 200,
        CancellationToken cancellationToken = default)
    {
        var limit = Math.Clamp(take, 1, 1000);

        var query = _dbContext.CompetitorFeatures
            .Include(f => f.ScrapedPage)
            .AsQueryable();

        if (pageId is not null)
            query = query.Where(f => f.ScrapedPageId == pageId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(f => EF.Functions.ILike(f.Name, pattern) || EF.Functions.ILike(f.Description, pattern));
        }

        var features = await query
            .OrderByDescending(f => f.ExtractedAt)
            .Take(limit)
            .Select(f => new CompetitorFeatureResponse(
                f.Id, f.Name, f.Description, f.Category,
                f.KeyBenefits, f.UseCases, f.Differentiators, f.TargetAudience,
                f.ScrapedPage.Url, f.ScrapedPage.Title, f.ExtractedAt))
            .ToListAsync(cancellationToken);

        return Ok(features);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CompetitorFeatureResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var feature = await _dbContext.CompetitorFeatures
            .Include(f => f.ScrapedPage)
            .Where(f => f.Id == id)
            .Select(f => new CompetitorFeatureResponse(
                f.Id, f.Name, f.Description, f.Category,
                f.KeyBenefits, f.UseCases, f.Differentiators, f.TargetAudience,
                f.ScrapedPage.Url, f.ScrapedPage.Title, f.ExtractedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return feature is null ? NotFound() : Ok(feature);
    }
}
