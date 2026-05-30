using BacklogOptimizer.Core.Common;
using BacklogOptimizer.Core.Entities;
using BacklogOptimizer.Infrastructure.Persistence;
using BacklogOptimizer.Infrastructure.Prompts;
using BacklogOptimizer.UnitTests.Helpers;

namespace BacklogOptimizer.UnitTests.Infrastructure.Prompts;

public class PromptServiceTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly PromptService _service;
    private readonly PromptResources _resources;

    public PromptServiceTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _resources = new PromptResources();
        _service = new PromptService(_dbContext, _resources);
    }

    public void Dispose() => _dbContext.Dispose();

    private async Task SeedTemplateAsync(PromptType type)
    {
        var template = new PromptTemplate(type, _resources.GetDefaultInstructions(type));
        _dbContext.PromptTemplates.Add(template);
        await _dbContext.SaveChangesAsync();
    }

    // -- GetAsync --

    [Fact]
    public async Task GetAsync_ExistingTemplate_ReturnsView()
    {
        await SeedTemplateAsync(PromptType.FeatureSuggestion);

        var result = await _service.GetAsync(PromptType.FeatureSuggestion);

        Assert.True(result.IsSuccess);
        Assert.Equal(PromptType.FeatureSuggestion, result.Value!.Type);
        Assert.NotEmpty(result.Value.Instructions);
    }

    [Fact]
    public async Task GetAsync_MissingTemplate_ReturnsNotFoundError()
    {
        var result = await _service.GetAsync(PromptType.Reprioritization);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    // -- GetAllAsync --

    [Fact]
    public async Task GetAllAsync_NoTemplates_ReturnsEmpty()
    {
        var result = await _service.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTemplates()
    {
        await SeedTemplateAsync(PromptType.FeatureSuggestion);
        await SeedTemplateAsync(PromptType.Reprioritization);

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    // -- UpdateInstructionsAsync --

    [Fact]
    public async Task UpdateInstructionsAsync_ValidInstructions_UpdatesTemplate()
    {
        await SeedTemplateAsync(PromptType.FeatureSuggestion);

        var result = await _service.UpdateInstructionsAsync(
            PromptType.FeatureSuggestion, "New instructions here.", "user-1");

        Assert.True(result.IsSuccess);
        var view = await _service.GetAsync(PromptType.FeatureSuggestion);
        Assert.Equal("New instructions here.", view.Value!.Instructions);
    }

    [Fact]
    public async Task UpdateInstructionsAsync_EmptyInstructions_ReturnsValidationError()
    {
        await SeedTemplateAsync(PromptType.FeatureSuggestion);

        var result = await _service.UpdateInstructionsAsync(
            PromptType.FeatureSuggestion, "   ", null);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    [Fact]
    public async Task UpdateInstructionsAsync_MissingTemplate_ReturnsNotFoundError()
    {
        var result = await _service.UpdateInstructionsAsync(
            PromptType.Reprioritization, "Some instructions.", null);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    // -- ResetToDefaultAsync --

    [Fact]
    public async Task ResetToDefaultAsync_ExistingTemplate_RestoresDefault()
    {
        await SeedTemplateAsync(PromptType.FeatureSuggestion);
        await _service.UpdateInstructionsAsync(PromptType.FeatureSuggestion, "Custom!", null);
        var defaultInstructions = _resources.GetDefaultInstructions(PromptType.FeatureSuggestion);

        var result = await _service.ResetToDefaultAsync(PromptType.FeatureSuggestion, "user-1");

        Assert.True(result.IsSuccess);
        var view = await _service.GetAsync(PromptType.FeatureSuggestion);
        Assert.Equal(defaultInstructions, view.Value!.Instructions);
    }

    [Fact]
    public async Task ResetToDefaultAsync_MissingTemplate_ReturnsNotFoundError()
    {
        var result = await _service.ResetToDefaultAsync(PromptType.FeatureSuggestion, null);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    // -- BuildSystemPromptAsync --

    [Fact]
    public async Task BuildSystemPromptAsync_NoCustomTemplate_UsesDefaultInstructions()
    {
        var prompt = await _service.BuildSystemPromptAsync(PromptType.FeatureSuggestion);

        Assert.Contains(_resources.GetDefaultInstructions(PromptType.FeatureSuggestion), prompt);
        Assert.Contains(_resources.GetSchema(PromptType.FeatureSuggestion), prompt);
    }

    [Fact]
    public async Task BuildSystemPromptAsync_WithCustomTemplate_UsesCustomInstructions()
    {
        await SeedTemplateAsync(PromptType.Reprioritization);
        await _service.UpdateInstructionsAsync(PromptType.Reprioritization, "Custom reprioritization.", null);

        var prompt = await _service.BuildSystemPromptAsync(PromptType.Reprioritization);

        Assert.Contains("Custom reprioritization.", prompt);
    }
}
