using BacklogOptimizer.Api.Extensions;
using BacklogOptimizer.Api.Models;
using BacklogOptimizer.Application.Auth;
using BacklogOptimizer.Application.Jira;
using BacklogOptimizer.Infrastructure.Persistence;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BacklogOptimizer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class SyncController : ControllerBase
{
    private readonly IJiraSyncService _jiraSyncService;
    private readonly ApplicationDbContext _dbContext;

    public SyncController(IJiraSyncService jiraSyncService, ApplicationDbContext dbContext)
    {
        _jiraSyncService = jiraSyncService;
        _dbContext = dbContext;
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(SyncResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Sync(CancellationToken cancellationToken)
    {
        var result = await _jiraSyncService.SyncAsync(cancellationToken);
        return result.ToActionResult(r => Ok(new SyncResponse(r.SyncedCount, r.Errors)));
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(SyncStatusResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
    {
        var total = await _dbContext.JiraIssues.CountAsync(cancellationToken);
        var lastSync = await _dbContext.JiraIssues
            .OrderByDescending(j => j.LastSyncedAt)
            .Select(j => (DateTime?)j.LastSyncedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return Ok(new SyncStatusResponse(total, lastSync));
    }
}
