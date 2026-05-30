using BacklogOptimizer.Core.Entities;

namespace BacklogOptimizer.UnitTests.Core.Entities;

public class PromptTemplateTests
{
    [Fact]
    public void Constructor_SetsTypeAndInstructions()
    {
        var template = new PromptTemplate(PromptType.FeatureSuggestion, "Do something.");

        Assert.Equal(PromptType.FeatureSuggestion, template.Type);
        Assert.Equal("Do something.", template.Instructions);
        Assert.Null(template.UpdatedByUserId);
    }

    [Fact]
    public void UpdateInstructions_ChangesInstructionsAndUserId()
    {
        var template = new PromptTemplate(PromptType.Reprioritization, "Old instructions");

        template.UpdateInstructions("New instructions", "user-123");

        Assert.Equal("New instructions", template.Instructions);
        Assert.Equal("user-123", template.UpdatedByUserId);
    }

    [Fact]
    public void UpdateInstructions_NullUserId_Allowed()
    {
        var template = new PromptTemplate(PromptType.FeatureExtraction, "instructions");

        template.UpdateInstructions("updated", null);

        Assert.Null(template.UpdatedByUserId);
    }
}

