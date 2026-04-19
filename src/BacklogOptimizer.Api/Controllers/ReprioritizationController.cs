using BacklogOptimizer.Api.Extensions;
using BacklogOptimizer.Api.Models;
using BacklogOptimizer.Application.Analysis;
using BacklogOptimizer.Infrastructure.Persistence;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BacklogOptimizer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ReprioritizationController : ControllerBase
{
    private readonly IReprioritizationService _service;
    private readonly ApplicationDbContext _dbContext;

    public ReprioritizationController(IReprioritizationService service, ApplicationDbContext dbContext)
    {
        _service = service;
        _dbContext = dbContext;
    }

    [HttpPost("analyze")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Analyze(CancellationToken cancellationToken)
    {
        var result = await _service.EnqueueAnalysisAsync(cancellationToken);
        return result.ToActionResult(StatusCodes.Status202Accepted);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ReprioritizationSuggestionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var items = await _dbContext.ReprioritizationSuggestions
            .OrderByDescending(r => r.ConfidenceScore)
            .Select(r => new ReprioritizationSuggestionResponse(
                r.JiraKey, r.CurrentPriority, r.SuggestedPriority,
                r.Reasoning, r.CompetitorEvidence, r.ConfidenceScore, r.AnalyzedAt))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }
}
