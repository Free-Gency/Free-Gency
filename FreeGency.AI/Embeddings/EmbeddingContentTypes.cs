namespace FreeGency.AI.Embeddings;

public static class EmbeddingContentTypes
{
    public const string ProjectDescription = "project_description";
    public const string CoverLetter = "cover_letter";
    public const string PortfolioSummary = "portfolio_summary";
    public const string Skills = "skills";
    public const string DeveloperProfile = "developer_profile";
    public const string TeamProfile = "team_profile";
    public const string TeamJob = "team_job";
    public const string ProjectListing = "project_listing";

    private static readonly Dictionary<string, string> Prefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        [ProjectDescription] = "Classify this project description for matching: ",
        [CoverLetter] = "Evaluate this proposal cover letter for relevance: ",
        [PortfolioSummary] = "Summarize this developer portfolio for ranking: ",
        [Skills] = "Encode these skills for similarity search: ",
        [DeveloperProfile] = "Encode this developer profile for team and job matching: ",
        [TeamProfile] = "Encode this team profile for project matching: ",
        [TeamJob] = "Encode this open team job for developer matching: ",
        [ProjectListing] = "Encode this published project listing for candidate matching: "
    };

    public static string GetPrefix(string contentType)
        => Prefixes.TryGetValue(contentType, out var prefix) ? prefix : string.Empty;
}
