using FreeGency.AI.Core;
using FreeGency.AI.Interfaces;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace FreeGency.AI.Embeddings;

public sealed class EmbeddingService : IEmbeddingService
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _generator;
    private readonly AIOptions _options;

    public int Dimension => _options.EmbeddingDimension;

    public EmbeddingService(
        IEmbeddingGenerator<string, Embedding<float>> generator,
        IOptions<AIOptions> options)
    {
        _generator = generator;
        _options = options.Value;
    }

    public async Task<float[]> EmbedProjectDescriptionAsync(string description, CancellationToken ct = default)
    {
        var prefixed = EmbeddingContentTypes.GetPrefix(EmbeddingContentTypes.ProjectDescription) + description;
        return await GenerateEmbeddingInternalAsync(prefixed, documentMode: true, ct);
    }

    public async Task<float[]> EmbedCoverLetterAsync(string coverLetter, CancellationToken ct = default)
    {
        var prefixed = EmbeddingContentTypes.GetPrefix(EmbeddingContentTypes.CoverLetter) + coverLetter;
        return await GenerateEmbeddingInternalAsync(prefixed, documentMode: true, ct);
    }

    public async Task<float[]> EmbedPortfolioSummaryAsync(string portfolioSummary, CancellationToken ct = default)
    {
        var prefixed = EmbeddingContentTypes.GetPrefix(EmbeddingContentTypes.PortfolioSummary) + portfolioSummary;
        return await GenerateEmbeddingInternalAsync(prefixed, documentMode: true, ct);
    }

    public async Task<float[]> EmbedSkillsAsync(IEnumerable<string> skills, CancellationToken ct = default)
    {
        var joined = string.Join(", ", skills.Where(s => !string.IsNullOrWhiteSpace(s)));
        var prefixed = EmbeddingContentTypes.GetPrefix(EmbeddingContentTypes.Skills) + joined;
        return await GenerateEmbeddingInternalAsync(prefixed, documentMode: true, ct);
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    {
        // Default to document mode (indexing / general). Query path can override via batch options.
        return await GenerateEmbeddingInternalAsync(text, documentMode: true, ct);
    }

    public async Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken ct = default)
    {
        var textList = texts.ToList();
        if (textList.Count == 0)
            return Array.Empty<float[]>();

        var result = await GenerateAsync(textList, documentMode: true, ct);
        return result.Select(e => e.Vector.ToArray()).ToList();
    }

    public Task<double> ComputeCosineSimilarityAsync(float[] vectorA, float[] vectorB, CancellationToken ct = default)
    {
        if (vectorA.Length != vectorB.Length)
            throw new ArgumentException($"Vector dimensions mismatch: {vectorA.Length} vs {vectorB.Length}.");

        double dot = 0, normA = 0, normB = 0;

        for (int i = 0; i < vectorA.Length; i++)
        {
            dot += vectorA[i] * vectorB[i];
            normA += vectorA[i] * vectorA[i];
            normB += vectorB[i] * vectorB[i];
        }

        var similarity = (normA == 0 || normB == 0) ? 0.0 : dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
        return Task.FromResult(similarity);
    }

    private async Task<float[]> GenerateEmbeddingInternalAsync(string text, bool documentMode, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new float[Dimension];

        var result = await GenerateAsync([text], documentMode, ct);

        var embedding = result.FirstOrDefault()
            ?? throw new InvalidOperationException("Embedding generator returned no results.");

        return embedding.Vector.ToArray();
    }

    private async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IList<string> texts,
        bool documentMode,
        CancellationToken ct)
    {
        if (_generator is GeminiEmbeddingService gemini)
        {
            var taskType = documentMode
                ? GeminiEmbeddingService.TaskTypeDocument
                : GeminiEmbeddingService.TaskTypeQuery;
            return await gemini.GenerateWithTaskTypeAsync(texts, taskType, ct);
        }

        if (_generator is BedrockEmbeddingService bedrock)
        {
            var inputType = documentMode
                ? BedrockEmbeddingService.InputTypeDocument
                : BedrockEmbeddingService.InputTypeQuery;
            return await bedrock.GenerateWithInputTypeAsync(texts, inputType, ct);
        }

        return await _generator.GenerateAsync(texts, options: null, cancellationToken: ct);
    }
}
