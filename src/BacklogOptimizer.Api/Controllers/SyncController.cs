using BacklogOptimizer.Api.Extensions;
using BacklogOptimizer.Api.Models;
using BacklogOptimizer.Application.Auth;
using BacklogOptimizer.Application.Jira;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BacklogOptimizer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin)]
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
        return result.ToActionResult(r => Ok(new SyncResponse(r.SyncedCount, r.Errors)));
    }
}
