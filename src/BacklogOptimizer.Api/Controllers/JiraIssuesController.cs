using BacklogOptimizer.Api.Models;
using BacklogOptimizer.Infrastructure.Persistence;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BacklogOptimizer.Api.Controllers;

[ApiController]
[Route("api/jira-issues")]
[Authorize]
public sealed class JiraIssuesController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public JiraIssuesController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<JiraIssueResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null,
        [FromQuery] string? search = null,
        [FromQuery] int take = 200,
        CancellationToken cancellationToken = default)
    {
        var limit = Math.Clamp(take, 1, 1000);

        var query = _dbContext.JiraIssues.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(j => j.Status == status);

        if (!string.IsNullOrWhiteSpace(priority))
            query = query.Where(j => j.Priority == priority);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(j =>
                EF.Functions.ILike(j.JiraKey, pattern)
                || EF.Functions.ILike(j.Summary, pattern));
        }

        var issues = await query
            .OrderByDescending(j => j.JiraUpdatedAt)
            .Take(limit)
            .Select(j => new JiraIssueResponse(
                j.Id, j.JiraKey, j.Summary, j.Description,
                j.Status, j.Priority, j.IssueType, j.AssigneeEmail,
                j.JiraCreatedAt, j.JiraUpdatedAt, j.LastSyncedAt))
            .ToListAsync(cancellationToken);

        return Ok(issues);
    }
}
