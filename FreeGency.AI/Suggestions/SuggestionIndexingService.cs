using FreeGency.AI.Embeddings;
using FreeGency.AI.Interfaces;
using Microsoft.Extensions.Logging;

namespace FreeGency.AI.Suggestions;

public interface ISuggestionIndexingService
{
    Task UpsertAsync(BuiltSuggestionDocument document, CancellationToken ct = default);
    Task UpsertBatchAsync(IEnumerable<BuiltSuggestionDocument> documents, CancellationToken ct = default);
    Task DeleteAsync(string collection, string id, CancellationToken ct = default);
    Task ResetCollectionsAsync(CancellationToken ct = default);

    /// <summary>
    /// Embeds all documents first, then wipes collections and upserts.
    /// Avoids leaving Qdrant empty if Gemini/DB work fails before vectors are ready.
    /// </summary>
    Task RebuildCollectionsAsync(IEnumerable<BuiltSuggestionDocument> documents, CancellationToken ct = default);
}

public sealed class SuggestionIndexingService : ISuggestionIndexingService
{
    private readonly IEmbeddingService _embeddings;
    private readonly IVectorStore _vectorStore;
    private readonly ILogger<SuggestionIndexingService> _logger;

    public SuggestionIndexingService(
        IEmbeddingService embeddings,
        IVectorStore vectorStore,
        ILogger<SuggestionIndexingService> logger)
    {
        _embeddings = embeddings;
        _vectorStore = vectorStore;
        _logger = logger;
    }

    public async Task UpsertAsync(BuiltSuggestionDocument document, CancellationToken ct = default)
    {
        var vector = await EmbedDocumentAsync(document, ct);
        await _vectorStore.UpsertAsync(document.Collection, document.Id, vector, document.Payload, ct);
        _logger.LogDebug("Upserted suggestion vector {Id} into {Collection}.", document.Id, document.Collection);
    }

    public async Task UpsertBatchAsync(IEnumerable<BuiltSuggestionDocument> documents, CancellationToken ct = default)
    {
        foreach (var group in documents.GroupBy(d => d.Collection, StringComparer.OrdinalIgnoreCase))
        {
            var docs = group.ToList();
            if (docs.Count == 0)
                continue;

            var texts = docs
                .Select(d => EmbeddingContentTypes.GetPrefix(d.ContentType) + d.Text)
                .ToList();

            var vectors = await _embeddings.EmbedBatchAsync(texts, ct);
            var entries = new List<VectorStoreEntry>(docs.Count);

            for (var i = 0; i < docs.Count; i++)
            {
                entries.Add(new VectorStoreEntry
                {
                    Id = docs[i].Id,
                    Vector = vectors[i],
                    Metadata = docs[i].Payload
                });
            }

            await _vectorStore.UpsertBatchAsync(group.Key, entries, ct);
            _logger.LogInformation("Upserted {Count} suggestion vectors into {Collection}.", entries.Count, group.Key);
        }
    }

    public Task DeleteAsync(string collection, string id, CancellationToken ct = default)
        => _vectorStore.DeleteAsync(collection, id, ct);

    public async Task ResetCollectionsAsync(CancellationToken ct = default)
    {
        foreach (var collection in SuggestionCollections.All)
        {
            await _vectorStore.DeleteCollectionAsync(collection, ct);
        }
    }

    public async Task RebuildCollectionsAsync(IEnumerable<BuiltSuggestionDocument> documents, CancellationToken ct = default)
    {
        var prepared = new List<(string Collection, List<VectorStoreEntry> Entries)>();

        foreach (var group in documents.GroupBy(d => d.Collection, StringComparer.OrdinalIgnoreCase))
        {
            var docs = group.ToList();
            if (docs.Count == 0)
                continue;

            var texts = docs
                .Select(d => EmbeddingContentTypes.GetPrefix(d.ContentType) + d.Text)
                .ToList();

            var vectors = await _embeddings.EmbedBatchAsync(texts, ct);
            var entries = new List<VectorStoreEntry>(docs.Count);
            for (var i = 0; i < docs.Count; i++)
            {
                entries.Add(new VectorStoreEntry
                {
                    Id = docs[i].Id,
                    Vector = vectors[i],
                    Metadata = docs[i].Payload
                });
            }

            prepared.Add((group.Key, entries));
        }

        // Wipe only after embeddings succeeded — otherwise For you stays populated.
        await ResetCollectionsAsync(ct);

        foreach (var (collection, entries) in prepared)
        {
            await _vectorStore.UpsertBatchAsync(collection, entries, ct);
            _logger.LogInformation(
                "Rebuilt {Count} suggestion vectors into {Collection}.",
                entries.Count,
                collection);
        }
    }

    private async Task<float[]> EmbedDocumentAsync(BuiltSuggestionDocument document, CancellationToken ct)
    {
        var prefixed = EmbeddingContentTypes.GetPrefix(document.ContentType) + document.Text;
        return await _embeddings.EmbedAsync(prefixed, ct);
    }
}
