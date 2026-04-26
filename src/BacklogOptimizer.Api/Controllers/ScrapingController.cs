using BacklogOptimizer.Api.Extensions;
using BacklogOptimizer.Api.Models;
using BacklogOptimizer.Application.Auth;
using BacklogOptimizer.Application.Scraping;
using BacklogOptimizer.Infrastructure.Persistence;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BacklogOptimizer.Api.Controllers;

[ApiController]
[Route("api/scraping")]
[Authorize]
public sealed class ScrapingController : ControllerBase
{
    private readonly IScrapingService _scrapingService;
    private readonly ApplicationDbContext _dbContext;

    public ScrapingController(IScrapingService scrapingService, ApplicationDbContext dbContext)
    {
        _scrapingService = scrapingService;
        _dbContext = dbContext;
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnqueueScrape([FromBody] ScrapeRequest request, CancellationToken cancellationToken)
    {
        var result = await _scrapingService.EnqueueScrapeAsync(request.Url, cancellationToken);
        return result.ToActionResult(StatusCodes.Status202Accepted);
    }

    [HttpGet("jobs")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(IReadOnlyList<ScrapingJobResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetJobs([FromQuery] int take = 50, CancellationToken cancellationToken = default)
    {
        var limit = Math.Clamp(take, 1, 200);
        var jobs = await _dbContext.ScrapingJobs
            .OrderByDescending(j => j.CreatedAt)
            .Take(limit)
            .Select(j => new ScrapingJobResponse(
                j.Id, j.Url, j.Status.ToString(), j.AttemptCount, j.ErrorMessage,
                j.CreatedAt, j.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Ok(jobs);
    }

    [HttpGet("pages")]
    [ProducesResponseType(typeof(IReadOnlyList<ScrapedPageResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPages([FromQuery] int take = 100, CancellationToken cancellationToken = default)
    {
        var limit = Math.Clamp(take, 1, 500);
        var pages = await _dbContext.ScrapedPages
            .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
            .Take(limit)
            .Select(p => new ScrapedPageResponse(
                p.Id, p.Url, p.Title, p.Features.Count, p.CreatedAt, p.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Ok(pages);
    }
}
