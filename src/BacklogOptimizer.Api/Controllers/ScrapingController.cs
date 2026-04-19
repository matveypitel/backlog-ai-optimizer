using BacklogOptimizer.Api.Extensions;
using BacklogOptimizer.Api.Models;
using BacklogOptimizer.Application.Scraping;

using Microsoft.AspNetCore.Mvc;

namespace BacklogOptimizer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ScrapingController : ControllerBase
{
    private readonly IScrapingService _scrapingService;

    public ScrapingController(IScrapingService scrapingService)
    {
        _scrapingService = scrapingService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnqueueScrape([FromBody] ScrapeRequest request, CancellationToken cancellationToken)
    {
        var result = await _scrapingService.EnqueueScrapeAsync(request.Url, cancellationToken);
        return result.ToActionResult(StatusCodes.Status202Accepted);
    }
}
