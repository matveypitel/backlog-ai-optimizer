using BacklogOptimizer.Core.Common;

namespace BacklogOptimizer.Core.Entities;

public class PromptTemplate : BaseEntity
{
    public PromptType Type { get; private set; }
    public string Instructions { get; private set; }
    public string? UpdatedByUserId { get; private set; }

    private PromptTemplate() { Instructions = string.Empty; }

    public PromptTemplate(PromptType type, string instructions)
    {
        Type = type;
        Instructions = instructions;
    }

    public void UpdateInstructions(string instructions, string? updatedByUserId)
    {
        Instructions = instructions;
        UpdatedByUserId = updatedByUserId;
    }
}
