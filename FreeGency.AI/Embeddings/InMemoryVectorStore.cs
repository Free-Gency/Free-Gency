using FreeGency.AI.Interfaces;

namespace FreeGency.AI.Embeddings;

public sealed class InMemoryVectorStore : IVectorStore
{
    private readonly Dictionary<string, Dictionary<string, VectorStoreEntry>> _collections = new();

    public Task UpsertAsync(string collection, string id, float[] vector, IDictionary<string, string>? metadata = null, CancellationToken ct = default)
    {
        if (!_collections.ContainsKey(collection))
            _collections[collection] = new Dictionary<string, VectorStoreEntry>();

        _collections[collection][id] = new VectorStoreEntry { Id = id, Vector = vector, Metadata = metadata };
        return Task.CompletedTask;
    }

    public Task UpsertBatchAsync(string collection, IEnumerable<VectorStoreEntry> entries, CancellationToken ct = default)
    {
        if (!_collections.ContainsKey(collection))
            _collections[collection] = new Dictionary<string, VectorStoreEntry>();

        foreach (var entry in entries)
            _collections[collection][entry.Id] = entry;

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        string collection,
        float[] queryVector,
        int topK = 10,
        double? minScore = null,
        IReadOnlyDictionary<string, string>? payloadFilters = null,
        CancellationToken ct = default)
    {
        if (!_collections.TryGetValue(collection, out var items))
            return Task.FromResult<IReadOnlyList<VectorSearchResult>>([]);

        var results = items.Values
            .Where(e => MatchesFilters(e.Metadata, payloadFilters))
            .Select(e => new VectorSearchResult
            {
                Id = e.Id,
                Score = (float)CosineSimilarity(queryVector, e.Vector),
                Metadata = e.Metadata
            })
            .OrderByDescending(r => r.Score)
            .Where(r => minScore is null || r.Score >= minScore)
            .Take(topK)
            .ToList();

        return Task.FromResult<IReadOnlyList<VectorSearchResult>>(results);
    }

    public Task DeleteAsync(string collection, string id, CancellationToken ct = default)
    {
        if (_collections.TryGetValue(collection, out var items))
            items.Remove(id);

        return Task.CompletedTask;
    }

    public Task DeleteCollectionAsync(string collection, CancellationToken ct = default)
    {
        _collections.Remove(collection);
        return Task.CompletedTask;
    }

    public Task<bool> CollectionExistsAsync(string collection, CancellationToken ct = default)
    {
        return Task.FromResult(_collections.ContainsKey(collection));
    }

    private static bool MatchesFilters(IDictionary<string, string>? metadata, IReadOnlyDictionary<string, string>? filters)
    {
        if (filters is null || filters.Count == 0)
            return true;

        if (metadata is null)
            return false;

        foreach (var filter in filters)
        {
            if (!metadata.TryGetValue(filter.Key, out var value) ||
                !string.Equals(value, filter.Value, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static double CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Vectors must have the same length.");

        double dot = 0, normA = 0, normB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        return normA == 0 || normB == 0 ? 0 : dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }
}
