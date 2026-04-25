using System.Reflection;

using BacklogOptimizer.Core.Entities;

namespace BacklogOptimizer.Infrastructure.Prompts;

internal sealed class PromptResources
{
    private readonly Dictionary<PromptType, (string Schema, string DefaultInstructions)> _cache;

    public PromptResources()
    {
        _cache = new Dictionary<PromptType, (string, string)>
        {
            [PromptType.FeatureExtraction] = Load("feature-extraction"),
            [PromptType.FeatureSuggestion] = Load("feature-suggestion"),
            [PromptType.Reprioritization] = Load("reprioritization")
        };
    }

    public string GetSchema(PromptType type) => _cache[type].Schema;

    public string GetDefaultInstructions(PromptType type) => _cache[type].DefaultInstructions;

    private static (string Schema, string DefaultInstructions) Load(string baseName)
    {
        var schema = ReadResource($"{baseName}.schema.md");
        var instructions = ReadResource($"{baseName}.instructions.default.md");
        return (schema, instructions);
    }

    private static string ReadResource(string fileName)
    {
        var assembly = typeof(PromptResources).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Embedded prompt resource '{fileName}' not found.");

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not open embedded prompt resource '{resourceName}'.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
