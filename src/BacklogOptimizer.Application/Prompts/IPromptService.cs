using BacklogOptimizer.Core.Common;
using BacklogOptimizer.Core.Entities;

namespace BacklogOptimizer.Application.Prompts;

public interface IPromptService
{
    Task<string> BuildSystemPromptAsync(PromptType type, CancellationToken cancellationToken = default);

    Task<Result<PromptTemplateView>> GetAsync(PromptType type, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PromptTemplateView>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result> UpdateInstructionsAsync(PromptType type, string instructions, string? updatedByUserId, CancellationToken cancellationToken = default);

    Task<Result> ResetToDefaultAsync(PromptType type, string? updatedByUserId, CancellationToken cancellationToken = default);
}
