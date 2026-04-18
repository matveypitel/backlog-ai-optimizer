using BacklogOptimizer.Api.Models;
using BacklogOptimizer.Application.Embeddings;

using Microsoft.AspNetCore.Mvc;

namespace BacklogOptimizer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    [ProducesResponseType(typeof(EmbeddingSyncResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SyncJira(CancellationToken cancellationToken)
    {
        var result = await _syncService.SyncJiraEmbeddingsAsync(cancellationToken);
        return Ok(new EmbeddingSyncResponse(result.EmbeddedCount, result.SkippedCount, result.Errors));
    }

    [HttpPost("sync/pages")]
    [ProducesResponseType(typeof(EmbeddingSyncResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SyncPages(CancellationToken cancellationToken)
    {
        var result = await _syncService.SyncPageEmbeddingsAsync(cancellationToken);
        return Ok(new EmbeddingSyncResponse(result.EmbeddedCount, result.SkippedCount, result.Errors));
    }

    [HttpPost("search")]
    [ProducesResponseType(typeof(IReadOnlyList<SimilarityMatch>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromBody] EmbeddingSearchRequest request, CancellationToken cancellationToken)
    {
        var matches = await _searchService.FindSimilarPagesAsync(request.JiraKey, request.TopN, cancellationToken);
        return Ok(matches);
    }
}
