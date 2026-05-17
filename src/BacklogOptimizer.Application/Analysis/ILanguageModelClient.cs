namespace BacklogOptimizer.Application.Analysis;

public interface ILanguageModelClient
{
    Task<string> CompleteAsync(
        string model,
        IEnumerable<LanguageModelMessage> messages,
        CancellationToken cancellationToken = default);
    Task<string> CompleteAsync(
        string model,
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        var messages = new[]
        {
            new LanguageModelMessage("system", systemPrompt),
            new LanguageModelMessage("user", userPrompt)
        };

        return CompleteAsync(model, messages, cancellationToken);
    }
}

public class LanguageModelMessage
{
    public string Role { get; set; }
    public string Content { get; set; }

    public LanguageModelMessage(string role, string content)
    {
        Role = role;
        Content = content;
    }
}
