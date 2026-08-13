using FreeGency.AI.Core;
using FreeGency.AI.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace FreeGency.AI.Embeddings;

public sealed class QdrantVectorStore : IVectorStore, IDisposable
{
    private readonly QdrantClient _client;
    private readonly int _dimension;
    private readonly ILogger<QdrantVectorStore> _logger;
    private readonly HashSet<string> _ensuredCollections = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim _ensureLock = new(1, 1);

    public QdrantVectorStore(IOptions<AIOptions> options, ILogger<QdrantVectorStore> logger)
    {
        _logger = logger;
        var ai = options.Value;
        _dimension = ai.EmbeddingDimension > 0 ? ai.EmbeddingDimension : AIConstants.DefaultEmbeddingDimension;

        var vectorOptions = ai.VectorStore;
        if (string.IsNullOrWhiteSpace(vectorOptions.ConnectionString))
            throw new ArgumentException("AI:VectorStore:ConnectionString must be configured for Qdrant.");

        _client = CreateClient(vectorOptions);
    }

    public async Task UpsertAsync(string collection, string id, float[] vector, IDictionary<string, string>? metadata = null, CancellationToken ct = default)
    {
        await EnsureCollectionAsync(collection, ct);

        var point = new PointStruct
        {
            Id = ToPointId(id),
            Vectors = vector,
            Payload = { }
        };

        foreach (var (key, value) in metadata ?? new Dictionary<string, string>())
            point.Payload[key] = value;

        await _client.UpsertAsync(collection, [point], cancellationToken: ct);
    }

    public async Task UpsertBatchAsync(string collection, IEnumerable<VectorStoreEntry> entries, CancellationToken ct = default)
    {
        var entryList = entries.ToList();
        if (entryList.Count == 0)
            return;

        await EnsureCollectionAsync(collection, ct);

        var points = entryList.Select(entry =>
        {
            var point = new PointStruct
            {
                Id = ToPointId(entry.Id),
                Vectors = entry.Vector,
                Payload = { }
            };

            if (entry.Metadata is not null)
            {
                foreach (var (key, value) in entry.Metadata)
                    point.Payload[key] = value;
            }

            return point;
        }).ToList();

        const int batchSize = 64;
        for (var i = 0; i < points.Count; i += batchSize)
        {
            var batch = points.Skip(i).Take(batchSize).ToList();
            await _client.UpsertAsync(collection, batch, cancellationToken: ct);
        }
    }

    public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        string collection,
        float[] queryVector,
        int topK = 10,
        double? minScore = null,
        IReadOnlyDictionary<string, string>? payloadFilters = null,
        CancellationToken ct = default)
    {
        if (!await CollectionExistsAsync(collection, ct))
            return [];

        Filter? filter = null;
        if (payloadFilters is { Count: > 0 })
        {
            filter = new Filter();
            foreach (var (key, value) in payloadFilters)
            {
                filter.Must.Add(new Condition
                {
                    Field = new FieldCondition
                    {
                        Key = key,
                        Match = new Match { Keyword = value }
                    }
                });
            }
        }

        try
        {
            return await SearchInternalAsync(collection, queryVector, topK, minScore, filter, ct);
        }
        catch (Exception ex) when (filter is not null)
        {
            // Qdrant Cloud may reject filtered search without a payload index.
            _logger.LogWarning(
                ex,
                "Filtered search on '{Collection}' failed; retrying without filter.",
                collection);
            return await SearchInternalAsync(collection, queryVector, topK, minScore, null, ct);
        }
    }

    private async Task<IReadOnlyList<VectorSearchResult>> SearchInternalAsync(
        string collection,
        float[] queryVector,
        int topK,
        double? minScore,
        Filter? filter,
        CancellationToken ct)
    {
        var hits = await _client.SearchAsync(
            collectionName: collection,
            vector: queryVector,
            filter: filter,
            limit: (uint)Math.Max(1, topK),
            scoreThreshold: minScore.HasValue ? (float)minScore.Value : null,
            payloadSelector: true,
            cancellationToken: ct);

        return hits.Select(hit => new VectorSearchResult
        {
            Id = FromPointId(hit.Id),
            Score = hit.Score,
            Metadata = ToMetadata(hit.Payload)
        }).ToList();
    }

    public async Task DeleteAsync(string collection, string id, CancellationToken ct = default)
    {
        if (!await CollectionExistsAsync(collection, ct))
            return;

        await _client.DeleteAsync(collection, ids: [ToPointId(id)], cancellationToken: ct);
    }

    public async Task DeleteCollectionAsync(string collection, CancellationToken ct = default)
    {
        if (!await CollectionExistsAsync(collection, ct))
            return;

        await _client.DeleteCollectionAsync(collection, cancellationToken: ct);

        await _ensureLock.WaitAsync(ct);
        try
        {
            _ensuredCollections.Remove(collection);
        }
        finally
        {
            _ensureLock.Release();
        }
    }

    public async Task<bool> CollectionExistsAsync(string collection, CancellationToken ct = default)
    {
        try
        {
            return await _client.CollectionExistsAsync(collection, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed checking Qdrant collection '{Collection}'.", collection);
            return false;
        }
    }

    public async Task<float[]?> GetVectorAsync(string collection, string id, CancellationToken ct = default)
    {
        if (!await CollectionExistsAsync(collection, ct))
            return null;

        try
        {
            var points = await _client.RetrieveAsync(
                collectionName: collection,
                ids: [ToPointId(id)],
                withVectors: true,
                withPayload: false,
                cancellationToken: ct);

            var point = points.FirstOrDefault();
            if (point?.Vectors is null)
                return null;

            // Dense vector (single unnamed vector) is the common FreeGency layout.
            if (point.Vectors.Vector?.Data is { Count: > 0 } data)
                return data.ToArray();

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "GetVector failed for {Collection}/{Id}.", collection, id);
            return null;
        }
    }

    public void Dispose()
    {
        _client.Dispose();
        _ensureLock.Dispose();
    }

    private async Task EnsureCollectionAsync(string collection, CancellationToken ct)
    {
        if (_ensuredCollections.Contains(collection))
            return;

        await _ensureLock.WaitAsync(ct);
        try
        {
            if (_ensuredCollections.Contains(collection))
                return;

            if (!await _client.CollectionExistsAsync(collection, cancellationToken: ct))
            {
                await _client.CreateCollectionAsync(
                    collectionName: collection,
                    vectorsConfig: new VectorParams
                    {
                        Size = (ulong)_dimension,
                        Distance = Distance.Cosine
                    },
                    cancellationToken: ct);

                _logger.LogInformation("Created Qdrant collection '{Collection}' (dim={Dimension}).", collection, _dimension);
            }

            // Keyword filter on team_jobs.status needs an index on Qdrant Cloud.
            if (string.Equals(collection, "team_jobs", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    await _client.CreatePayloadIndexAsync(
                        collectionName: collection,
                        fieldName: "status",
                        schemaType: PayloadSchemaType.Keyword,
                        cancellationToken: ct);
                }
                catch (Exception ex)
                {
                    // Already exists / not required — safe to ignore.
                    _logger.LogDebug(ex, "Payload index for '{Collection}.status' skipped.", collection);
                }
            }

            _ensuredCollections.Add(collection);
        }
        finally
        {
            _ensureLock.Release();
        }
    }

    private static QdrantClient CreateClient(VectorStoreOptions options)
    {
        var raw = options.ConnectionString.Trim();
        if (!raw.Contains("://", StringComparison.Ordinal))
            raw = "http://" + raw;

        var uri = new Uri(raw);
        var https = uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
        var port = uri.IsDefaultPort ? (https ? 443 : 6334) : uri.Port;

        // REST default is 6333; gRPC client expects 6334.
        if (port == 6333)
            port = 6334;

        var apiKey = string.IsNullOrWhiteSpace(options.ApiKey) ? null : options.ApiKey;
        return new QdrantClient(uri.Host, port, https, apiKey);
    }

    private static PointId ToPointId(string id)
    {
        if (Guid.TryParse(id, out var guid))
            return guid;

        // Deterministic UUID from arbitrary string ids.
        var bytes = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(id));
        return new Guid(bytes);
    }

    private static string FromPointId(PointId id)
    {
        if (!string.IsNullOrWhiteSpace(id.Uuid))
            return id.Uuid;

        return id.Num.ToString();
    }

    private static IDictionary<string, string>? ToMetadata(IDictionary<string, Value>? payload)
    {
        if (payload is null || payload.Count == 0)
            return null;

        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in payload)
            dict[key] = ValueToString(value);

        return dict;
    }

    private static string ValueToString(Value value)
    {
        return value.KindCase switch
        {
            Value.KindOneofCase.StringValue => value.StringValue,
            Value.KindOneofCase.IntegerValue => value.IntegerValue.ToString(),
            Value.KindOneofCase.DoubleValue => value.DoubleValue.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Value.KindOneofCase.BoolValue => value.BoolValue ? "true" : "false",
            _ => value.ToString()
        };
    }
}
