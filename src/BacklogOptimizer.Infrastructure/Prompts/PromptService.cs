using BacklogOptimizer.Application.Prompts;
using BacklogOptimizer.Core.Common;
using BacklogOptimizer.Core.Entities;
using BacklogOptimizer.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace BacklogOptimizer.Infrastructure.Prompts;

internal sealed class PromptService : IPromptService
{
    private const string SchemaSeparator = "\n\n---\nRESPONSE FORMAT (do not deviate):\n";

    private readonly ApplicationDbContext _dbContext;
    private readonly PromptResources _resources;

    public PromptService(ApplicationDbContext dbContext, PromptResources resources)
    {
        _dbContext = dbContext;
        _resources = resources;
    }

    public async Task<string> BuildSystemPromptAsync(PromptType type, CancellationToken cancellationToken = default)
    {
        var instructions = await GetInstructionsAsync(type, cancellationToken);
        var schema = _resources.GetSchema(type);
        return instructions + SchemaSeparator + schema;
    }

    public async Task<Result<PromptTemplateView>> GetAsync(PromptType type, CancellationToken cancellationToken = default)
    {
        var template = await _dbContext.PromptTemplates
            .FirstOrDefaultAsync(t => t.Type == type, cancellationToken);

        if (template is null)
            return new Error("prompt.not_found", ErrorType.NotFound, $"Prompt template '{type}' not found.");

        return ToView(template);
    }

    public async Task<IReadOnlyList<PromptTemplateView>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var templates = await _dbContext.PromptTemplates.ToListAsync(cancellationToken);
        return templates.Select(ToView).OrderBy(v => v.Type).ToList();
    }

    public async Task<Result> UpdateInstructionsAsync(PromptType type, string instructions, string? updatedByUserId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(instructions))
            return new Error("prompt.invalid_instructions", ErrorType.Validation, "Instructions must not be empty.");

        var template = await _dbContext.PromptTemplates
            .FirstOrDefaultAsync(t => t.Type == type, cancellationToken);

        if (template is null)
            return new Error("prompt.not_found", ErrorType.NotFound, $"Prompt template '{type}' not found.");

        template.UpdateInstructions(instructions.Trim(), updatedByUserId);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> ResetToDefaultAsync(PromptType type, string? updatedByUserId, CancellationToken cancellationToken = default)
    {
        var template = await _dbContext.PromptTemplates
            .FirstOrDefaultAsync(t => t.Type == type, cancellationToken);

        if (template is null)
            return new Error("prompt.not_found", ErrorType.NotFound, $"Prompt template '{type}' not found.");

        template.UpdateInstructions(_resources.GetDefaultInstructions(type), updatedByUserId);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<string> GetInstructionsAsync(PromptType type, CancellationToken cancellationToken)
    {
        var template = await _dbContext.PromptTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Type == type, cancellationToken);

        return template?.Instructions ?? _resources.GetDefaultInstructions(type);
    }

    private PromptTemplateView ToView(PromptTemplate template) => new(
        template.Type,
        template.Instructions,
        _resources.GetSchema(template.Type),
        _resources.GetDefaultInstructions(template.Type),
        template.UpdatedAt,
        template.UpdatedByUserId);
}
