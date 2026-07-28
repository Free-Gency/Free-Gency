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
        return await GenerateEmbeddingInternalAsync(prefixed, ct);
    }

    public async Task<float[]> EmbedCoverLetterAsync(string coverLetter, CancellationToken ct = default)
    {
        var prefixed = EmbeddingContentTypes.GetPrefix(EmbeddingContentTypes.CoverLetter) + coverLetter;
        return await GenerateEmbeddingInternalAsync(prefixed, ct);
    }

    public async Task<float[]> EmbedPortfolioSummaryAsync(string portfolioSummary, CancellationToken ct = default)
    {
        var prefixed = EmbeddingContentTypes.GetPrefix(EmbeddingContentTypes.PortfolioSummary) + portfolioSummary;
        return await GenerateEmbeddingInternalAsync(prefixed, ct);
    }

    public async Task<float[]> EmbedSkillsAsync(IEnumerable<string> skills, CancellationToken ct = default)
    {
        var joined = string.Join(", ", skills.Where(s => !string.IsNullOrWhiteSpace(s)));
        var prefixed = EmbeddingContentTypes.GetPrefix(EmbeddingContentTypes.Skills) + joined;
        return await GenerateEmbeddingInternalAsync(prefixed, ct);
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    {
        return await GenerateEmbeddingInternalAsync(text, ct);
    }

    public async Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken ct = default)
    {
        var textList = texts.ToList();
        if (textList.Count == 0)
            return Array.Empty<float[]>();

        var result = await _generator.GenerateAsync(
            textList,
            options: null,
            cancellationToken: ct);

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

    private async Task<float[]> GenerateEmbeddingInternalAsync(string text, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new float[Dimension];

        var result = await _generator.GenerateAsync(
            [text],
            options: null,
            cancellationToken: ct);

        var embedding = result.FirstOrDefault()
            ?? throw new InvalidOperationException("Embedding generator returned no results.");

        return embedding.Vector.ToArray();
    }
}
