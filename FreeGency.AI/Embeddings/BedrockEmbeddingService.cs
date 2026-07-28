using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

namespace FreeGency.AI.Embeddings;

public sealed class BedrockEmbeddingService : IEmbeddingGenerator<string, Embedding<float>>
{
    private readonly HttpClient _httpClient;
    private readonly string _modelId;

    public IReadOnlyDictionary<string, object?> Metadata { get; } = new Dictionary<string, object?>();

    public BedrockEmbeddingService(HttpClient httpClient, IConfiguration configuration)
    {
        var apiKey = configuration["SBG_API_KEY"]
                     ?? throw new InvalidOperationException("Missing SBG_API_KEY in appsettings");

        _modelId = configuration["AI:EmbeddingModelId"]
                   ?? "amazon.titan-embed-text-v2:0";

        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://apiaccess.iti.net.eg");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> inputs,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var inputList = inputs.ToList();
        var results = new List<Embedding<float>>(inputList.Count);

        foreach (var text in inputList)
        {
            var vector = await GenerateSingleEmbeddingAsync(text, cancellationToken);
            results.Add(new Embedding<float>(vector));
        }

        return new GeneratedEmbeddings<Embedding<float>>(results);
    }

    public object? GetService(Type serviceType, object? key = null)
    {
        return null;
    }

    public void Dispose()
    {
    }

    private async Task<float[]> GenerateSingleEmbeddingAsync(string text, CancellationToken ct)
    {
        var payload = new
        {
            model_id = _modelId,
            input_text = text
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("api/v1/student/embeddings", content, ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(ct);
        var doc = JsonDocument.Parse(body);

        if (!doc.RootElement.TryGetProperty("embedding", out var embeddingElement))
            throw new InvalidOperationException("Embedding response missing 'embedding' field.");

        return embeddingElement.EnumerateArray()
            .Select(e => e.GetSingle())
            .ToArray();
    }
}
