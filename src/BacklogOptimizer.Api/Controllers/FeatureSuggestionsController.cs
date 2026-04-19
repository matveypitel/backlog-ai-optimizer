using BacklogOptimizer.Api.Models;
using BacklogOptimizer.Application.Analysis;
using BacklogOptimizer.Infrastructure.Persistence;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BacklogOptimizer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class FeatureSuggestionsController : ControllerBase
{
    private readonly IFeatureSuggestionService _service;
    private readonly ApplicationDbContext _dbContext;

    public FeatureSuggestionsController(IFeatureSuggestionService service, ApplicationDbContext dbContext)
    {
        _service = service;
        _dbContext = dbContext;
    }

    [HttpPost("analyze")]
    [ProducesResponseType(typeof(FeatureSuggestionAnalyzeResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Analyze(CancellationToken cancellationToken)
    {
        var result = await _service.AnalyzeAsync(cancellationToken);
        return Ok(new FeatureSuggestionAnalyzeResponse(result.GapPagesFound, result.SuggestionsGenerated, result.Errors));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FeatureSuggestionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var items = await _dbContext.FeatureSuggestions
            .OrderBy(f => f.SuggestedPriority)
            .Select(f => new FeatureSuggestionResponse(
                f.Id, f.Title, f.Description, f.IssueType,
                f.SuggestedPriority, f.Tags, f.Reasoning,
                f.CompetitorEvidence, f.AnalyzedAt))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }
}
