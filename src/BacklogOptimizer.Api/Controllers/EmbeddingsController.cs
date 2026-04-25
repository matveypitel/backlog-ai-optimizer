using BacklogOptimizer.Api.Extensions;
using BacklogOptimizer.Api.Models;
using BacklogOptimizer.Application.Auth;
using BacklogOptimizer.Application.Embeddings;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BacklogOptimizer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class EmbeddingsController : ControllerBase
{
    private readonly IEmbeddingSyncService _syncService;
    private readonly ISimilaritySearchService _searchService;

    public EmbeddingsController(IEmbeddingSyncService syncService, ISimilaritySearchService searchService)
    {
        _syncService = syncService;
        _searchService = searchService;
    }

    [HttpPost("sync/jira")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(EmbeddingSyncResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SyncJira(CancellationToken cancellationToken)
    {
        var result = await _syncService.SyncJiraEmbeddingsAsync(cancellationToken);
        return result.ToActionResult(r => Ok(new EmbeddingSyncResponse(r.EmbeddedCount, r.SkippedCount, r.Errors)));
    }

    [HttpPost("sync/features")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(EmbeddingSyncResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SyncFeatures(CancellationToken cancellationToken)
    {
        var result = await _syncService.SyncFeatureEmbeddingsAsync(cancellationToken);
        return result.ToActionResult(r => Ok(new EmbeddingSyncResponse(r.EmbeddedCount, r.SkippedCount, r.Errors)));
    }

    [HttpPost("search")]
    [ProducesResponseType(typeof(IReadOnlyList<SimilarityMatch>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Search([FromBody] EmbeddingSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _searchService.FindSimilarFeaturesAsync(request.JiraKey, request.TopN, cancellationToken);
        return result.ToActionResult();
    }
}
