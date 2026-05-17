using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using BacklogOptimizer.Application.Analysis;
using BacklogOptimizer.Infrastructure.Analysis.Dto;

namespace BacklogOptimizer.Infrastructure.Analysis;

internal sealed class OpenAiChatClient : ILanguageModelClient
{
    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public OpenAiChatClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> CompleteAsync(
        string model,
        IEnumerable<LanguageModelMessage> messages,
        CancellationToken cancellationToken = default)
    {
        var chatMessages = messages
            .Select(m => new ChatMessage(m.Role, m.Content))
            .ToList();

        var request = new ChatRequest(
            model,
            [.. chatMessages],
            new ChatResponseFormat("json_object"));

        var response = await _httpClient.PostAsJsonAsync("/v1/chat/completions", request, WriteOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"OpenAI chat completions API returned {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<ChatResponse>(ReadOptions, cancellationToken)
            ?? throw new InvalidOperationException("Empty response from OpenAI chat completions API");

        return result.Choices[0].Message.Content;
    }
}
