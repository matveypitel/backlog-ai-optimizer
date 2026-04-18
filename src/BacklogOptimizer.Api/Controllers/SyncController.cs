using BacklogOptimizer.Application.Jira;

using Microsoft.AspNetCore.Mvc;

namespace BacklogOptimizer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SyncController : ControllerBase
{
    private readonly IJiraSyncService _jiraSyncService;

    public SyncController(IJiraSyncService jiraSyncService)
    {
        _jiraSyncService = jiraSyncService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(SyncResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Sync(CancellationToken cancellationToken)
    {
        var result = await _jiraSyncService.SyncAsync(cancellationToken);

        if (result.SyncedCount == 0 && result.Errors.Count > 0)
            return BadRequest(new { errors = result.Errors });

        return Ok(new SyncResponse(result.SyncedCount, result.Errors));
    }
}
