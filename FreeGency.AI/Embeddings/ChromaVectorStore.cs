using ChromaDB.Client;
using ChromaDB.Client.Models;
using FreeGency.AI.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FreeGency.AI.Embeddings;

public sealed class ChromaVectorStore : IVectorStore
{
    private readonly ChromaClient _client;
    private readonly ChromaConfigurationOptions _configOptions;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ChromaVectorStore> _logger;
    private readonly Dictionary<string, ChromaCollectionClient> _collections = new();
    private readonly object _lock = new();

    public ChromaVectorStore(IOptions<Core.VectorStoreOptions> options, ILogger<ChromaVectorStore> logger)
    {
        _logger = logger;

        var uri = options.Value.ConnectionString;
        if (string.IsNullOrWhiteSpace(uri))
            throw new ArgumentException("VectorStore.ConnectionString (Chroma URI) must be configured.", nameof(options));

        _configOptions = new ChromaConfigurationOptions(uri);
        _httpClient = new HttpClient { BaseAddress = new Uri(uri) };
        _client = new ChromaClient(_configOptions, _httpClient);
    }

    public async Task UpsertAsync(string collection, string id, float[] vector, IDictionary<string, string>? metadata = null, CancellationToken ct = default)
    {
        var client = await GetCollectionClientAsync(collection);
        var embedding = new ReadOnlyMemory<float>(vector);
        var chromaMetadata = ToChromaMetadata(metadata);

        await client.Upsert(
            ids: [id],
            embeddings: [embedding],
            metadatas: chromaMetadata is not null ? [chromaMetadata] : null);
    }

    public async Task UpsertBatchAsync(string collection, IEnumerable<VectorStoreEntry> entries, CancellationToken ct = default)
    {
        var client = await GetCollectionClientAsync(collection);
        var entryList = entries.ToList();
        if (entryList.Count == 0) return;

        var ids = entryList.Select(e => e.Id).ToList();
        var embeddings = entryList.Select(e => new ReadOnlyMemory<float>(e.Vector)).ToList();
        var metadatas = entryList.Select(e => ToChromaMetadata(e.Metadata) ?? new Dictionary<string, object>()).ToList();

        await client.Upsert(
            ids: ids,
            embeddings: embeddings,
            metadatas: metadatas);
    }

    public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        string collection,
        float[] queryVector,
        int topK = 10,
        double? minScore = null,
        IReadOnlyDictionary<string, string>? payloadFilters = null,
        CancellationToken ct = default)
    {
        var client = await GetCollectionClientAsync(collection);
        var queryEmbedding = new ReadOnlyMemory<float>(queryVector);

        var results = await client.Query(
            queryEmbedding,
            nResults: Math.Max(topK * 3, topK),
            include: ChromaQueryInclude.Metadatas | ChromaQueryInclude.Distances);

        return results
            .Select(r => new VectorSearchResult
            {
                Id = r.Id,
                Score = (float)(1.0 / (1.0 + r.Distance)),
                Metadata = ToSearchMetadata(r.Metadata)
            })
            .Where(r => MatchesFilters(r.Metadata, payloadFilters))
            .Where(r => minScore is null || r.Score >= minScore)
            .Take(topK)
            .ToList();
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

    public async Task DeleteAsync(string collection, string id, CancellationToken ct = default)
    {
        var client = await GetCollectionClientAsync(collection);
        await client.Delete(ids: [id]);
    }

    public async Task DeleteCollectionAsync(string collection, CancellationToken ct = default)
    {
        await _client.DeleteCollection(collection);

        lock (_lock)
        {
            _collections.Remove(collection);
        }
    }

    public async Task<bool> CollectionExistsAsync(string collection, CancellationToken ct = default)
    {
        try
        {
            await _client.GetCollection(collection);
            return true;
        }
        catch (ChromaException)
        {
            return false;
        }
        catch (HttpRequestException)
        {
            _logger.LogWarning("Chroma server unreachable while checking collection '{Collection}'.", collection);
            return false;
        }
    }

    private async Task<ChromaCollectionClient> GetCollectionClientAsync(string collection)
    {
        lock (_lock)
        {
            if (_collections.TryGetValue(collection, out var cached))
                return cached;
        }

        var chromaCollection = await _client.GetOrCreateCollection(collection);
        var client = new ChromaCollectionClient(chromaCollection, _configOptions, _httpClient);

        lock (_lock)
        {
            if (!_collections.ContainsKey(collection))
                _collections[collection] = client;
            return _collections[collection];
        }
    }

    private static List<Dictionary<string, object>>? ToChromaMetadata(IEnumerable<IDictionary<string, string>?>? metadataSets)
    {
        if (metadataSets is null) return null;

        var result = metadataSets
            .Select(m => ToChromaMetadata(m) ?? new Dictionary<string, object>())
            .ToList();

        return result.Count > 0 ? result : null;
    }

    private static Dictionary<string, object>? ToChromaMetadata(IDictionary<string, string>? metadata)
    {
        if (metadata is null || metadata.Count == 0) return null;

        var dict = new Dictionary<string, object>();
        foreach (var kvp in metadata)
            dict[kvp.Key] = kvp.Value;

        return dict;
    }

    private static IDictionary<string, string>? ToSearchMetadata(Dictionary<string, object>? chromaMetadata)
    {
        if (chromaMetadata is null || chromaMetadata.Count == 0) return null;

        return chromaMetadata.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value?.ToString() ?? string.Empty);
    }
}
