using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using BacklogOptimizer.Infrastructure.Embeddings.Dto;

namespace BacklogOptimizer.Infrastructure.Embeddings;

internal sealed class OpenAiEmbeddingClient
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

    public OpenAiEmbeddingClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<float[]> GetEmbeddingAsync(string text, string model, CancellationToken cancellationToken)
    {
        var request = new EmbeddingRequest(text, model);
        var response = await _httpClient.PostAsJsonAsync("/v1/embeddings", request, WriteOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"OpenAI embeddings API returned {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(ReadOptions, cancellationToken)
            ?? throw new InvalidOperationException("Empty response from OpenAI embeddings API");

        return result.Data[0].Embedding;
    }
}
