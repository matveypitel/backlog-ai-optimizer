using BacklogOptimizer.Api.Extensions;
using BacklogOptimizer.Api.Models;
using BacklogOptimizer.Application.Analysis;
using BacklogOptimizer.Application.Suggestions;
using BacklogOptimizer.Infrastructure.Persistence;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BacklogOptimizer.Api.Controllers;

[ApiController]
[Route("api/feature-suggestions")]
[Authorize]
public sealed class FeatureSuggestionsController : ControllerBase
{
    private readonly IFeatureSuggestionService _service;
    private readonly ISuggestionApplicationService _applicationService;
    private readonly ApplicationDbContext _dbContext;

    public FeatureSuggestionsController(
        IFeatureSuggestionService service,
        ISuggestionApplicationService applicationService,
        ApplicationDbContext dbContext)
    {
        _service = service;
        _applicationService = applicationService;
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
    [ProducesResponseType(typeof(IReadOnlyList<FeatureSuggestionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var items = await _dbContext.FeatureSuggestions
            .Where(f => f.AppliedAt == null)
            .OrderBy(f => f.SuggestedPriority)
            .Select(f => new FeatureSuggestionResponse(
                f.Id, f.Title, f.Description, f.IssueType,
                f.SuggestedPriority, f.Tags, f.Reasoning,
                f.CompetitorEvidence, f.BusinessValue, f.EstimatedImpact,
                f.UserStories, f.AcceptanceCriteria, f.AnalyzedAt))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FeatureSuggestionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var item = await _dbContext.FeatureSuggestions
            .Where(f => f.Id == id && f.AppliedAt == null)
            .Select(f => new FeatureSuggestionResponse(
                f.Id, f.Title, f.Description, f.IssueType,
                f.SuggestedPriority, f.Tags, f.Reasoning,
                f.CompetitorEvidence, f.BusinessValue, f.EstimatedImpact,
                f.UserStories, f.AcceptanceCriteria, f.AnalyzedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("{id:guid}/apply")]
    [ProducesResponseType(typeof(ApplyFeatureSuggestionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Apply(Guid id, CancellationToken cancellationToken)
    {
        var result = await _applicationService.ApplyFeatureSuggestionAsync(id, cancellationToken);
        return result.ToActionResult(value => Ok(new ApplyFeatureSuggestionResponse(value.JiraKey)));
    }

    [HttpGet("jobs/latest")]
    [ProducesResponseType(typeof(JobStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLatestJob(CancellationToken cancellationToken)
    {
        var job = await _dbContext.FeatureSuggestionJobs
            .OrderByDescending(j => j.CreatedAt)
            .Select(j => new JobStatusResponse(j.Id, j.Status.ToString(), j.ErrorMessage, j.CreatedAt, j.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return job is null ? NotFound() : Ok(job);
    }
}
