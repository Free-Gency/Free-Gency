namespace FreeGency.AI.Core;

public sealed class AIOptions
{
    public const string SectionName = "AI";

    public string DefaultModelId { get; set; } = string.Empty;
    public int MaxTokens { get; set; } = 800;
    public double Temperature { get; set; } = 0.7;
    /// <summary>gemini | bedrock | local</summary>
    public string EmbeddingProvider { get; set; } = "gemini";
    public string EmbeddingModelId { get; set; } = "gemini-embedding-001";
    public string GeminiApiKey { get; set; } = string.Empty;
    public int EmbeddingDimension { get; set; } = AIConstants.DefaultEmbeddingDimension;
    public CacheOptions Cache { get; set; } = new();
    public VectorStoreOptions VectorStore { get; set; } = new();
}

public sealed class CacheOptions
{
    public bool Enabled { get; set; } = true;
    public int ExpirationMinutes { get; set; } = AIConstants.DefaultCacheExpirationMinutes;
    public int MaxEntries { get; set; } = 1000;
}

public sealed class VectorStoreOptions
{
    public string Provider { get; set; } = "inmemory";
    public string ConnectionString { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int DefaultTopK { get; set; } = AIConstants.DefaultTopK;
    public double SimilarityThreshold { get; set; } = AIConstants.DefaultSimilarityThreshold;
}
