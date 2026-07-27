namespace FreeGency.AI.Interfaces;

public interface IEmbeddingService
{
    Task<float[]> EmbedProjectDescriptionAsync(string description, CancellationToken ct = default);
    Task<float[]> EmbedCoverLetterAsync(string coverLetter, CancellationToken ct = default);
    Task<float[]> EmbedPortfolioSummaryAsync(string portfolioSummary, CancellationToken ct = default);
    Task<float[]> EmbedSkillsAsync(IEnumerable<string> skills, CancellationToken ct = default);
    Task<float[]> EmbedAsync(string text, CancellationToken ct = default);
    Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken ct = default);
    Task<double> ComputeCosineSimilarityAsync(float[] vectorA, float[] vectorB, CancellationToken ct = default);
    int Dimension { get; }
}
