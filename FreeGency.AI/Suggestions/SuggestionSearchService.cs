using System.Globalization;
using FreeGency.AI.Core;
using FreeGency.AI.Embeddings;
using FreeGency.AI.Interfaces;
using Microsoft.Extensions.Options;

namespace FreeGency.AI.Suggestions;

public interface ISuggestionSearchService
{
    Task<IReadOnlyList<ScoredSuggestion>> SearchTeamJobsForDeveloperAsync(
        DeveloperSuggestionDocument developer,
        int topK,
        CancellationToken ct = default);

    Task<IReadOnlyList<ScoredSuggestion>> SearchCandidatesForProjectAsync(
        ProjectSuggestionDocument project,
        int topK,
        CancellationToken ct = default);
}

public sealed class ScoredSuggestion
{
    public required string Id { get; init; }
    public required string CandidateType { get; init; }
    public float VectorScore { get; init; }
    public float FinalScore { get; init; }
    public IDictionary<string, string>? Metadata { get; init; }
    public ScoreBreakdown Breakdown { get; init; } = new();
}

public sealed class ScoreBreakdown
{
    public float Vector { get; init; }
    public float SkillOverlap { get; init; }
    public float SpecialtyOverlap { get; init; }
    public float Rating { get; init; }
    public float Portfolio { get; init; }
}

public sealed class SuggestionSearchService : ISuggestionSearchService
{
    private readonly SuggestionDocumentBuilder _builder;
    private readonly IEmbeddingService _embeddings;
    private readonly IVectorStore _vectorStore;
    private readonly VectorStoreOptions _options;

    public SuggestionSearchService(
        SuggestionDocumentBuilder builder,
        IEmbeddingService embeddings,
        IVectorStore vectorStore,
        IOptions<AIOptions> options)
    {
        _builder = builder;
        _embeddings = embeddings;
        _vectorStore = vectorStore;
        _options = options.Value.VectorStore;
    }

    public async Task<IReadOnlyList<ScoredSuggestion>> SearchTeamJobsForDeveloperAsync(
        DeveloperSuggestionDocument developer,
        int topK,
        CancellationToken ct = default)
    {
        topK = NormalizeTopK(topK);
        var built = _builder.BuildDeveloper(developer);
        var queryVector = await EmbedBuiltAsync(built, ct);

        // Only open jobs are indexed in team_jobs (closed jobs are deleted),
        // so avoid a Qdrant payload filter — Cloud often rejects keyword filters
        // without a payload index, which breaks Teams → For you while Client For you
        // (unfiltered teams/developers search) still works.
        var recallK = Math.Max(topK * 4, topK);
        var hits = await _vectorStore.SearchAsync(
            SuggestionCollections.TeamJobs,
            queryVector,
            recallK,
            _options.SimilarityThreshold,
            null,
            ct);

        var developerSkills = developer.SkillIds.ToHashSet();
        var scored = hits.Select(hit =>
        {
            var jobSkills = ParseIds(hit.Metadata, "skillIds");
            var skillOverlap = OverlapRatio(developerSkills, jobSkills);
            var rating = NormalizeRating(
                ParseDecimal(hit.Metadata, "teamAverageRating"),
                ParseInt(hit.Metadata, "teamRatingCount"));
            var portfolio = string.Equals(
                GetMeta(hit.Metadata, "teamHasPublicPortfolio"),
                "true",
                StringComparison.OrdinalIgnoreCase)
                ? 1f
                : 0f;

            var final = (0.55f * hit.Score)
                        + (0.25f * skillOverlap)
                        + (0.15f * rating)
                        + (0.05f * portfolio);

            return new ScoredSuggestion
            {
                Id = hit.Id,
                CandidateType = "TeamJob",
                VectorScore = hit.Score,
                FinalScore = final,
                Metadata = hit.Metadata,
                Breakdown = new ScoreBreakdown
                {
                    Vector = hit.Score,
                    SkillOverlap = skillOverlap,
                    Rating = rating,
                    Portfolio = portfolio
                }
            };
        })
        .GroupBy(s => GetMeta(s.Metadata, "teamId"), StringComparer.OrdinalIgnoreCase)
        .Select(g => g.OrderByDescending(x => x.FinalScore).First())
        .OrderByDescending(s => s.FinalScore)
        .Take(topK)
        .ToList();

        return scored;
    }

    public async Task<IReadOnlyList<ScoredSuggestion>> SearchCandidatesForProjectAsync(
        ProjectSuggestionDocument project,
        int topK,
        CancellationToken ct = default)
    {
        topK = NormalizeTopK(topK);
        var built = _builder.BuildProject(project);
        var queryVector = await EmbedBuiltAsync(built, ct);
        var recallK = Math.Max(topK * 3, topK);

        var teamTask = _vectorStore.SearchAsync(
            SuggestionCollections.Teams,
            queryVector,
            recallK,
            _options.SimilarityThreshold,
            null,
            ct);

        var developerTask = _vectorStore.SearchAsync(
            SuggestionCollections.Developers,
            queryVector,
            recallK,
            _options.SimilarityThreshold,
            null,
            ct);

        await Task.WhenAll(teamTask, developerTask);

        var projectSkills = project.SkillIds.ToHashSet();
        var projectSpecialties = project.SpecialtyIds.ToHashSet();

        var teamScores = teamTask.Result.Select(hit =>
            ScoreCandidate(hit, "Team", projectSkills, projectSpecialties));

        var developerScores = developerTask.Result.Select(hit =>
            ScoreCandidate(hit, "Developer", projectSkills, projectSpecialties));

        return teamScores
            .Concat(developerScores)
            .OrderByDescending(s => s.FinalScore)
            .Take(topK)
            .ToList();
    }

    private ScoredSuggestion ScoreCandidate(
        VectorSearchResult hit,
        string candidateType,
        HashSet<Guid> projectSkills,
        HashSet<Guid> projectSpecialties)
    {
        var skillOverlap = OverlapRatio(projectSkills, ParseIds(hit.Metadata, "skillIds"));
        var specialtyOverlap = OverlapRatio(projectSpecialties, ParseIds(hit.Metadata, "specialtyIds"));
        var rating = NormalizeRating(
            ParseDecimal(hit.Metadata, "averageRating"),
            ParseInt(hit.Metadata, "ratingCount"));

        var final = (0.55f * hit.Score)
                    + (0.25f * skillOverlap)
                    + (0.10f * specialtyOverlap)
                    + (0.10f * rating);

        return new ScoredSuggestion
        {
            Id = hit.Id,
            CandidateType = candidateType,
            VectorScore = hit.Score,
            FinalScore = final,
            Metadata = hit.Metadata,
            Breakdown = new ScoreBreakdown
            {
                Vector = hit.Score,
                SkillOverlap = skillOverlap,
                SpecialtyOverlap = specialtyOverlap,
                Rating = rating
            }
        };
    }

    private async Task<float[]> EmbedBuiltAsync(BuiltSuggestionDocument built, CancellationToken ct)
    {
        var prefixed = EmbeddingContentTypes.GetPrefix(built.ContentType) + built.Text;
        return await _embeddings.EmbedAsync(prefixed, ct);
    }

    private int NormalizeTopK(int topK)
    {
        if (topK <= 0)
            return _options.DefaultTopK > 0 ? _options.DefaultTopK : AIConstants.DefaultTopK;

        return Math.Clamp(topK, 1, 50);
    }

    private static float OverlapRatio(HashSet<Guid> left, HashSet<Guid> right)
    {
        if (left.Count == 0 || right.Count == 0)
            return 0f;

        var intersection = left.Count(right.Contains);
        return (float)intersection / left.Count;
    }

    private static float NormalizeRating(decimal averageRating, int ratingCount)
    {
        if (averageRating <= 0)
            return 0f;

        var confidence = Math.Clamp(ratingCount / 10f, 0.25f, 1f);
        return (float)(averageRating / 5m) * confidence;
    }

    private static HashSet<Guid> ParseIds(IDictionary<string, string>? metadata, string key)
    {
        var raw = GetMeta(metadata, key);
        if (string.IsNullOrWhiteSpace(raw))
            return [];

        return raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => Guid.TryParse(part, out var id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .ToHashSet();
    }

    private static decimal ParseDecimal(IDictionary<string, string>? metadata, string key)
    {
        var raw = GetMeta(metadata, key);
        return decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0m;
    }

    private static int ParseInt(IDictionary<string, string>? metadata, string key)
    {
        var raw = GetMeta(metadata, key);
        return int.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
    }

    private static string GetMeta(IDictionary<string, string>? metadata, string key)
    {
        if (metadata is null)
            return string.Empty;

        return metadata.TryGetValue(key, out var value) ? value : string.Empty;
    }
}
