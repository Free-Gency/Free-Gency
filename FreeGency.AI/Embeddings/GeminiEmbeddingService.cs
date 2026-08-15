using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FreeGency.AI.Core;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FreeGency.AI.Embeddings;

/// <summary>
/// Google Gemini embeddings via Generative Language API.
/// Model: gemini-embedding-001 (same as Docsense).
/// </summary>
public sealed class GeminiEmbeddingService : IEmbeddingGenerator<string, Embedding<float>>
{
    public const string TaskTypeDocument = "RETRIEVAL_DOCUMENT";
    public const string TaskTypeQuery = "RETRIEVAL_QUERY";

    private readonly HttpClient _httpClient;
    private readonly string _modelId;
    private readonly int _dimension;
    private readonly string _apiKey;
    private readonly ILogger<GeminiEmbeddingService> _logger;

    public IReadOnlyDictionary<string, object?> Metadata { get; } = new Dictionary<string, object?>();

    public GeminiEmbeddingService(
        HttpClient httpClient,
        IConfiguration configuration,
        IOptions<AIOptions> options,
        ILogger<GeminiEmbeddingService> logger)
    {
        _apiKey = configuration["GEMINI_API_KEY"]
                  ?? configuration["AI:GeminiApiKey"]
                  ?? string.Empty;

        _modelId = configuration["AI:EmbeddingModelId"]
                   ?? options.Value.EmbeddingModelId
                   ?? "gemini-embedding-001";

        _dimension = options.Value.EmbeddingDimension > 0
            ? options.Value.EmbeddingDimension
            : 1536;

        _logger = logger;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var taskType = ResolveTaskType(options);
        return GenerateWithTaskTypeAsync(values, taskType, cancellationToken);
    }

    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateWithTaskTypeAsync(
        IEnumerable<string> values,
        string taskType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException(
                "Gemini API key is missing. Set environment variable GEMINI_API_KEY or AI:GeminiApiKey in appsettings.");
        }

        var texts = values.ToList();
        var vectors = new List<Embedding<float>>(texts.Count);

        // Keep batches small to stay under free-tier rate limits.
        const int batchSize = 4;
        for (var offset = 0; offset < texts.Count; offset += batchSize)
        {
            var batch = texts.Skip(offset).Take(batchSize).ToList();
            var embedded = await EmbedBatchAsync(batch, taskType, cancellationToken);
            vectors.AddRange(embedded.Select(v => new Embedding<float>(v)));

            if (offset + batchSize < texts.Count)
                await Task.Delay(800, cancellationToken);
        }

        return new GeneratedEmbeddings<Embedding<float>>(vectors);
    }

    public object? GetService(Type serviceType, object? key = null) => null;

    public void Dispose()
    {
    }

    private async Task<IReadOnlyList<float[]>> EmbedBatchAsync(
        IReadOnlyList<string> texts,
        string taskType,
        CancellationToken ct)
    {
        if (texts.Count == 1)
            return [await EmbedSingleAsync(texts[0], taskType, ct)];

        var modelPath = $"models/{_modelId}";
        var requests = texts.Select(text => new Dictionary<string, object?>
        {
            ["model"] = modelPath,
            ["content"] = new
            {
                parts = new[] { new { text } }
            },
            ["taskType"] = taskType,
            ["outputDimensionality"] = _dimension
        }).ToList();

        var payload = new { requests };
        var json = JsonSerializer.Serialize(payload);
        var url = $"v1beta/{modelPath}:batchEmbedContents?key={Uri.EscapeDataString(_apiKey)}";

        const int maxAttempts = 6;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(body);
                if (!doc.RootElement.TryGetProperty("embeddings", out var embeddings) ||
                    embeddings.ValueKind != JsonValueKind.Array)
                {
                    throw new InvalidOperationException("Gemini batchEmbed response missing embeddings[].");
                }

                return embeddings.EnumerateArray()
                    .Select(e => NormalizeIfNeeded(ParseValues(e)))
                    .ToList();
            }

            _logger.LogError("Gemini batchEmbed failed ({Status}): {Body}", (int)response.StatusCode, body);

            if ((int)response.StatusCode is 400 or 404)
            {
                var fallback = new List<float[]>(texts.Count);
                foreach (var text in texts)
                    fallback.Add(await EmbedSingleAsync(text, taskType, ct));
                return fallback;
            }

            if ((int)response.StatusCode == 429 && attempt < maxAttempts)
            {
                var delayMs = Math.Min(30_000, 1_000 * (1 << attempt));
                _logger.LogWarning(
                    "Gemini rate limited on batchEmbed (attempt {Attempt}/{Max}). Waiting {Delay}ms.",
                    attempt, maxAttempts, delayMs);
                await Task.Delay(delayMs, ct);
                continue;
            }

            response.EnsureSuccessStatusCode();
        }

        throw new InvalidOperationException("Gemini batchEmbed failed after retries.");
    }

    private async Task<float[]> EmbedSingleAsync(string text, string taskType, CancellationToken ct)
    {
        var modelPath = $"models/{_modelId}";
        var payload = new
        {
            content = new
            {
                parts = new[] { new { text } }
            },
            taskType,
            outputDimensionality = _dimension
        };

        var json = JsonSerializer.Serialize(payload);
        var url = $"v1beta/{modelPath}:embedContent?key={Uri.EscapeDataString(_apiKey)}";

        const int maxAttempts = 6;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(body);
                if (!doc.RootElement.TryGetProperty("embedding", out var embedding))
                    throw new InvalidOperationException("Gemini embedContent response missing embedding.");

                return NormalizeIfNeeded(ParseValues(embedding));
            }

            _logger.LogError("Gemini embedContent failed ({Status}): {Body}", (int)response.StatusCode, body);

            if ((int)response.StatusCode == 429 && attempt < maxAttempts)
            {
                var delayMs = Math.Min(30_000, 1_000 * (1 << attempt));
                _logger.LogWarning(
                    "Gemini rate limited on embedContent (attempt {Attempt}/{Max}). Waiting {Delay}ms.",
                    attempt, maxAttempts, delayMs);
                await Task.Delay(delayMs, ct);
                continue;
            }

            response.EnsureSuccessStatusCode();
        }

        throw new InvalidOperationException("Gemini embedContent failed after retries.");
    }

    private static float[] ParseValues(JsonElement embeddingElement)
    {
        var valuesElement = embeddingElement.ValueKind == JsonValueKind.Object &&
                            embeddingElement.TryGetProperty("values", out var nested)
            ? nested
            : embeddingElement;

        return valuesElement.EnumerateArray()
            .Select(v => v.GetSingle())
            .ToArray();
    }

    /// <summary>
    /// gemini-embedding-001 requires manual L2 normalization when outputDimensionality != 3072.
    /// </summary>
    private float[] NormalizeIfNeeded(float[] vector)
    {
        if (_dimension >= 3072)
            return vector;

        double norm = 0;
        for (var i = 0; i < vector.Length; i++)
            norm += vector[i] * vector[i];

        norm = Math.Sqrt(norm);
        if (norm <= 0)
            return vector;

        for (var i = 0; i < vector.Length; i++)
            vector[i] = (float)(vector[i] / norm);

        return vector;
    }

    private static string ResolveTaskType(EmbeddingGenerationOptions? options)
    {
        if (options?.AdditionalProperties is not null &&
            options.AdditionalProperties.TryGetValue("task_type", out var value) &&
            value is string s &&
            !string.IsNullOrWhiteSpace(s))
        {
            return s;
        }

        return TaskTypeDocument;
    }
}
