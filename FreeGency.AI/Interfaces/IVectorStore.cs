namespace FreeGency.AI.Interfaces;

public interface IVectorStore
{
    Task UpsertAsync(string collection, string id, float[] vector, IDictionary<string, string>? metadata = null, CancellationToken ct = default);
    Task UpsertBatchAsync(string collection, IEnumerable<VectorStoreEntry> entries, CancellationToken ct = default);
    Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        string collection,
        float[] queryVector,
        int topK = 10,
        double? minScore = null,
        IReadOnlyDictionary<string, string>? payloadFilters = null,
        CancellationToken ct = default);
    Task DeleteAsync(string collection, string id, CancellationToken ct = default);
    Task DeleteCollectionAsync(string collection, CancellationToken ct = default);
    Task<bool> CollectionExistsAsync(string collection, CancellationToken ct = default);

    /// <summary>
    /// Returns the stored embedding for an indexed point, or null if missing.
    /// Used to avoid re-calling Gemini on every For you request.
    /// </summary>
    Task<float[]?> GetVectorAsync(string collection, string id, CancellationToken ct = default);
}

public sealed class VectorStoreEntry
{
    public required string Id { get; init; }
    public required float[] Vector { get; init; }
    public IDictionary<string, string>? Metadata { get; init; }
}

public sealed class VectorSearchResult
{
    public required string Id { get; init; }
    public required float Score { get; init; }
    public IDictionary<string, string>? Metadata { get; init; }
}
