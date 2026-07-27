namespace FreeGency.AI.Embeddings;

public static class EmbeddingContentTypes
{
    public const string ProjectDescription = "project_description";
    public const string CoverLetter = "cover_letter";
    public const string PortfolioSummary = "portfolio_summary";
    public const string Skills = "skills";

    private static readonly Dictionary<string, string> Prefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        [ProjectDescription] = "Classify this project description for matching: ",
        [CoverLetter] = "Evaluate this proposal cover letter for relevance: ",
        [PortfolioSummary] = "Summarize this developer portfolio for ranking: ",
        [Skills] = "Encode these skills for similarity search: "
    };

    public static string GetPrefix(string contentType)
        => Prefixes.TryGetValue(contentType, out var prefix) ? prefix : string.Empty;
}
