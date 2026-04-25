using System.Security.Claims;

using BacklogOptimizer.Api.Extensions;
using BacklogOptimizer.Api.Models;
using BacklogOptimizer.Application.Auth;
using BacklogOptimizer.Application.Prompts;
using BacklogOptimizer.Core.Entities;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BacklogOptimizer.Api.Controllers;

[ApiController]
[Route("api/admin/prompts")]
[Authorize(Roles = Roles.Admin)]
public sealed class PromptsController : ControllerBase
{
    private readonly IPromptService _promptService;

    public PromptsController(IPromptService promptService)
    {
        _promptService = promptService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PromptResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var items = await _promptService.GetAllAsync(cancellationToken);
        return Ok(items.Select(ToResponse).ToList());
    }

    [HttpGet("{type}")]
    [ProducesResponseType(typeof(PromptResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(PromptType type, CancellationToken cancellationToken)
    {
        var result = await _promptService.GetAsync(type, cancellationToken);
        return result.ToActionResult(view => Ok(ToResponse(view)));
    }

    [HttpPut("{type}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(PromptType type, [FromBody] UpdatePromptRequest request, CancellationToken cancellationToken)
    {
        var result = await _promptService.UpdateInstructionsAsync(type, request.Instructions, GetUserId(), cancellationToken);
        return result.ToActionResult(StatusCodes.Status204NoContent);
    }

    [HttpPost("{type}/reset")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reset(PromptType type, CancellationToken cancellationToken)
    {
        var result = await _promptService.ResetToDefaultAsync(type, GetUserId(), cancellationToken);
        return result.ToActionResult(StatusCodes.Status204NoContent);
    }

    private string? GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

    private static PromptResponse ToResponse(PromptTemplateView view) => new(
        view.Type,
        view.Instructions,
        view.Schema,
        view.DefaultInstructions,
        view.UpdatedAt,
        view.UpdatedByUserId);
}
