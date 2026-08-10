using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using FreeGency.AI.Core;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace FreeGency.AI.Embeddings;

/// <summary>
/// Deterministic bag-of-tokens hashing into a fixed-dimension unit vector.
/// Used when the ITI Bedrock gateway has no approved embedding models.
/// </summary>
public sealed partial class LocalDeterministicEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    private readonly int _dimension;

    public IReadOnlyDictionary<string, object?> Metadata { get; } = new Dictionary<string, object?>();

    public LocalDeterministicEmbeddingGenerator(IOptions<AIOptions> options)
    {
        _dimension = options.Value.EmbeddingDimension > 0
            ? options.Value.EmbeddingDimension
            : AIConstants.DefaultEmbeddingDimension;
    }

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var results = values.Select(text => new Embedding<float>(Embed(text))).ToList();
        return Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(results));
    }

    public object? GetService(Type serviceType, object? key = null) => null;

    public void Dispose()
    {
    }

    private float[] Embed(string text)
    {
        var vector = new float[_dimension];
        if (string.IsNullOrWhiteSpace(text))
            return vector;

        var tokens = TokenRegex().Matches(text.ToLowerInvariant())
            .Select(m => m.Value)
            .Where(t => t.Length > 1)
            .ToList();

        if (tokens.Count == 0)
            return vector;

        foreach (var token in tokens)
        {
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash1 = MD5.HashData(bytes);
            var hash2 = SHA256.HashData(bytes);

            for (var i = 0; i < 4; i++)
            {
                var index = BitConverter.ToUInt32(hash1, i * 4) % (uint)_dimension;
                var sign = ((hash2[i] & 1) == 0) ? 1f : -1f;
                vector[index] += sign;
            }
        }

        double norm = 0;
        for (var i = 0; i < vector.Length; i++)
            norm += vector[i] * vector[i];

        norm = Math.Sqrt(norm);
        if (norm > 0)
        {
            for (var i = 0; i < vector.Length; i++)
                vector[i] = (float)(vector[i] / norm);
        }

        return vector;
    }

    [GeneratedRegex(@"[\p{L}\p{N}_+#.]+", RegexOptions.CultureInvariant)]
    private static partial Regex TokenRegex();
}
