using BacklogOptimizer.Api.Extensions;
using BacklogOptimizer.Application.Scraping;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BacklogOptimizer.Api.Controllers;

[ApiController]
[Route("api/scraping/sources")]
[Authorize]
public sealed class ScrapingSourcesController : ControllerBase
{
    private readonly IScrapingSourceService _service;

    public ScrapingSourcesController(IScrapingSourceService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ScrapingSourceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var items = await _service.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ScrapingSourceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateScrapingSourceCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(command, cancellationToken);
        return result.ToActionResult(dto => Created($"/api/scraping/sources/{dto.Id}", dto));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ScrapingSourceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateScrapingSourceCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(id, cancellationToken);
        return result.ToActionResult(StatusCodes.Status204NoContent);
    }

    [HttpPost("sync-all")]
    [ProducesResponseType(typeof(SyncAllResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> SyncAll(CancellationToken cancellationToken)
    {
        var result = await _service.SyncAllActiveAsync(cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/sync")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SyncOne(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.SyncOneAsync(id, cancellationToken);
        return result.ToActionResult(StatusCodes.Status202Accepted);
    }
}
