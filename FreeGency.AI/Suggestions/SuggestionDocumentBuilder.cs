using System.Globalization;
using System.Text;
using FreeGency.AI.Embeddings;

namespace FreeGency.AI.Suggestions;

/// <summary>
/// Builds deterministic text documents + Qdrant payloads for suggestion indexing/search.
/// </summary>
public sealed class SuggestionDocumentBuilder
{
    public BuiltSuggestionDocument BuildDeveloper(DeveloperSuggestionDocument doc)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"developer profile for user {doc.UserId}");
        if (!string.IsNullOrWhiteSpace(doc.Bio))
            sb.AppendLine($"bio: {doc.Bio.Trim()}");

        AppendNamedList(sb, "skills", doc.SkillNames);
        AppendNamedList(sb, "specialties", doc.SpecialtyNames);
        AppendNamedList(sb, "categories", doc.CategoryNames);
        AppendPortfolios(sb, doc.Portfolios);
        AppendRating(sb, doc.AverageRating, doc.RatingCount);

        return new BuiltSuggestionDocument
        {
            Id = doc.UserId.ToString(),
            Collection = SuggestionCollections.Developers,
            ContentType = EmbeddingContentTypes.DeveloperProfile,
            Text = sb.ToString(),
            Payload = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["type"] = "developer",
                ["userId"] = doc.UserId.ToString(),
                ["averageRating"] = FormatDecimal(doc.AverageRating),
                ["ratingCount"] = doc.RatingCount.ToString(CultureInfo.InvariantCulture),
                ["skillIds"] = JoinIds(doc.SkillIds),
                ["specialtyIds"] = JoinIds(doc.SpecialtyIds),
                ["categoryIds"] = JoinIds(doc.CategoryIds),
                ["portfolioCount"] = doc.Portfolios.Count.ToString(CultureInfo.InvariantCulture)
            }
        };
    }

    public BuiltSuggestionDocument BuildTeam(TeamSuggestionDocument doc)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"team: {doc.Name}");
        if (!string.IsNullOrWhiteSpace(doc.AboutUs))
            sb.AppendLine($"about: {doc.AboutUs.Trim()}");

        AppendNamedList(sb, "skills", doc.SkillNames);
        AppendNamedList(sb, "specialties", doc.SpecialtyNames);
        AppendNamedList(sb, "categories", doc.CategoryNames);
        AppendPortfolios(sb, doc.Portfolios);
        AppendRating(sb, doc.AverageRating, doc.RatingCount);

        return new BuiltSuggestionDocument
        {
            Id = doc.TeamId.ToString(),
            Collection = SuggestionCollections.Teams,
            ContentType = EmbeddingContentTypes.TeamProfile,
            Text = sb.ToString(),
            Payload = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["type"] = "team",
                ["teamId"] = doc.TeamId.ToString(),
                ["averageRating"] = FormatDecimal(doc.AverageRating),
                ["ratingCount"] = doc.RatingCount.ToString(CultureInfo.InvariantCulture),
                ["skillIds"] = JoinIds(doc.SkillIds),
                ["specialtyIds"] = JoinIds(doc.SpecialtyIds),
                ["categoryIds"] = JoinIds(doc.CategoryIds),
                ["hasOpenJobs"] = doc.HasOpenJobs ? "true" : "false",
                ["portfolioCount"] = doc.Portfolios.Count.ToString(CultureInfo.InvariantCulture)
            }
        };
    }

    public BuiltSuggestionDocument BuildTeamJob(TeamJobSuggestionDocument doc)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"open team job: {doc.Title}");
        if (!string.IsNullOrWhiteSpace(doc.Description))
            sb.AppendLine($"description: {doc.Description.Trim()}");

        AppendNamedList(sb, "required skills", doc.RequiredSkillNames);
        sb.AppendLine($"team: {doc.TeamName}");
        if (!string.IsNullOrWhiteSpace(doc.TeamAbout))
            sb.AppendLine($"team about: {Truncate(doc.TeamAbout, 400)}");

        AppendNamedList(sb, "team skills", doc.TeamSkillNames);
        AppendRating(sb, doc.TeamAverageRating, doc.TeamRatingCount);

        return new BuiltSuggestionDocument
        {
            Id = doc.JobId.ToString(),
            Collection = SuggestionCollections.TeamJobs,
            ContentType = EmbeddingContentTypes.TeamJob,
            Text = sb.ToString(),
            Payload = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["jobId"] = doc.JobId.ToString(),
                ["teamId"] = doc.TeamId.ToString(),
                ["status"] = doc.Status,
                ["skillIds"] = JoinIds(doc.RequiredSkillIds),
                ["teamAverageRating"] = FormatDecimal(doc.TeamAverageRating),
                ["teamRatingCount"] = doc.TeamRatingCount.ToString(CultureInfo.InvariantCulture),
                ["teamHasPublicPortfolio"] = doc.TeamHasPublicPortfolio ? "true" : "false",
                ["teamName"] = doc.TeamName
            }
        };
    }

    public BuiltSuggestionDocument BuildProject(ProjectSuggestionDocument doc)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"published project: {doc.Title}");
        if (!string.IsNullOrWhiteSpace(doc.Description))
            sb.AppendLine($"description: {doc.Description.Trim()}");

        if (!string.IsNullOrWhiteSpace(doc.CategoryName))
            sb.AppendLine($"category: {doc.CategoryName}");

        AppendNamedList(sb, "skills", doc.SkillNames);
        AppendNamedList(sb, "specialties", doc.SpecialtyNames);
        sb.AppendLine($"budget: {FormatDecimal(doc.BudgetMin)}-{FormatDecimal(doc.BudgetMax)} {doc.Currency}");

        return new BuiltSuggestionDocument
        {
            Id = doc.ProjectId.ToString(),
            Collection = SuggestionCollections.Projects,
            ContentType = EmbeddingContentTypes.ProjectListing,
            Text = sb.ToString(),
            Payload = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["projectId"] = doc.ProjectId.ToString(),
                ["clientId"] = doc.ClientId.ToString(),
                ["status"] = doc.Status,
                ["skillIds"] = JoinIds(doc.SkillIds),
                ["specialtyIds"] = JoinIds(doc.SpecialtyIds),
                ["categoryId"] = doc.CategoryId.ToString(),
                ["budgetMin"] = FormatDecimal(doc.BudgetMin),
                ["budgetMax"] = FormatDecimal(doc.BudgetMax)
            }
        };
    }

    private static void AppendNamedList(StringBuilder sb, string label, IReadOnlyList<string> values)
    {
        if (values.Count == 0)
            return;

        sb.AppendLine($"{label}: {string.Join(", ", values.Where(v => !string.IsNullOrWhiteSpace(v)))}");
    }

    private static void AppendPortfolios(StringBuilder sb, IReadOnlyList<PortfolioSnippet> portfolios)
    {
        if (portfolios.Count == 0)
            return;

        sb.AppendLine("public portfolio:");
        foreach (var item in portfolios.Take(8))
        {
            sb.AppendLine($"- {item.Title}: {Truncate(item.Description, 220)}");
            if (item.SkillNames.Count > 0)
                sb.AppendLine($"  skills: {string.Join(", ", item.SkillNames)}");
        }
    }

    private static void AppendRating(StringBuilder sb, decimal averageRating, int ratingCount)
    {
        if (ratingCount <= 0 && averageRating <= 0)
            return;

        sb.AppendLine($"rating: {FormatDecimal(averageRating)} from {ratingCount} reviews");
    }

    private static string JoinIds(IReadOnlyList<Guid> ids) => string.Join(',', ids);

    private static string FormatDecimal(decimal value)
        => value.ToString("0.##", CultureInfo.InvariantCulture);

    private static string Truncate(string? text, int max)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var trimmed = text.Trim();
        return trimmed.Length <= max ? trimmed : trimmed[..max] + "...";
    }
}
