using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FreeGency.AI.Embeddings;

/// <summary>
/// ITI Student Bedrock Gateway embeddings client.
/// Matches official integration: POST /api/v1/student/embed
/// with { model_id, texts, input_type }.
/// </summary>
public sealed class BedrockEmbeddingService : IEmbeddingGenerator<string, Embedding<float>>
{
    public const string InputTypeDocument = "search_document";
    public const string InputTypeQuery = "search_query";

    private readonly HttpClient _httpClient;
    private readonly string _modelId;
    private readonly ILogger<BedrockEmbeddingService> _logger;

    public IReadOnlyDictionary<string, object?> Metadata { get; } = new Dictionary<string, object?>();

    public BedrockEmbeddingService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<BedrockEmbeddingService> logger)
    {
        var apiKey = configuration["SBG_API_KEY"]
                     ?? throw new InvalidOperationException("Missing SBG_API_KEY in configuration/user-secrets.");

        _modelId = configuration["AI:EmbeddingModelId"]
                   ?? "amazon.titan-embed-text-v2:0";
        _logger = logger;

        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://apiaccess.iti.net.eg");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> inputs,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var inputType = ResolveInputType(options);
        return GenerateWithInputTypeAsync(inputs, inputType, cancellationToken);
    }

    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateWithInputTypeAsync(
        IEnumerable<string> inputs,
        string inputType,
        CancellationToken cancellationToken = default)
    {
        var inputList = inputs.ToList();
        var results = new List<Embedding<float>>(inputList.Count);

        // Keep batches modest; gateway accepts texts[].
        const int batchSize = 16;
        for (var offset = 0; offset < inputList.Count; offset += batchSize)
        {
            var batch = inputList.Skip(offset).Take(batchSize).ToList();
            var vectors = await EmbedBatchAsync(batch, inputType, cancellationToken);
            results.AddRange(vectors.Select(v => new Embedding<float>(v)));
        }

        return new GeneratedEmbeddings<Embedding<float>>(results);
    }

    public object? GetService(Type serviceType, object? key = null) => null;

    public void Dispose()
    {
    }

    private async Task<IReadOnlyList<float[]>> EmbedBatchAsync(
        IReadOnlyList<string> texts,
        string inputType,
        CancellationToken ct)
    {
        // Official ITI payload shape from Student Bedrock Gateway docs.
        var payload = new
        {
            model_id = _modelId,
            texts,
            input_type = inputType
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("api/v1/student/embed", content, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "ITI embed failed ({Status}) model={Model} input_type={InputType}: {Body}",
                (int)response.StatusCode,
                _modelId,
                inputType,
                body);
            response.EnsureSuccessStatusCode();
        }

        using var doc = JsonDocument.Parse(body);
        return ParseEmbeddings(doc.RootElement, texts.Count);
    }

    private static string ResolveInputType(EmbeddingGenerationOptions? options)
    {
        if (options?.AdditionalProperties is not null &&
            options.AdditionalProperties.TryGetValue("input_type", out var value) &&
            value is string s &&
            !string.IsNullOrWhiteSpace(s))
        {
            return s;
        }

        return InputTypeDocument;
    }

    private static IReadOnlyList<float[]> ParseEmbeddings(JsonElement root, int expectedCount)
    {
        if (root.TryGetProperty("embeddings", out var embeddingsElement) &&
            embeddingsElement.ValueKind == JsonValueKind.Array)
        {
            return embeddingsElement.EnumerateArray()
                .Select(ParseSingleVector)
                .ToList();
        }

        if (root.TryGetProperty("data", out var dataElement) &&
            dataElement.ValueKind == JsonValueKind.Array)
        {
            return dataElement.EnumerateArray()
                .Select(ParseSingleVector)
                .ToList();
        }

        if (root.TryGetProperty("embedding", out var embeddingElement))
        {
            var single = ParseSingleVector(embeddingElement);
            return Enumerable.Repeat(single, expectedCount).ToList();
        }

        throw new InvalidOperationException("Embedding response missing 'embeddings'/'embedding' field.");
    }

    private static float[] ParseSingleVector(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (element.TryGetProperty("embedding", out var nested))
                element = nested;
            else if (element.TryGetProperty("vector", out var vector))
                element = vector;
        }

        return element.EnumerateArray()
            .Select(e => e.GetSingle())
            .ToArray();
    }
}
