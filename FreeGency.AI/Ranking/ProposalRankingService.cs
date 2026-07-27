using System.Diagnostics;
using System.Text;
using System.Text.Json;
using FreeGency.AI.Interfaces;
using FreeGency.AI.Prompts;
using FreeGency.AI.Ranking.ProposalRanking;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FreeGency.AI.Ranking;

public sealed class ProposalRankingService : IProposalRankingService
{
    private readonly IChatCompletionService _chat;

    public ProposalRankingService(IChatCompletionService chat)
    {
        _chat = chat;
    }

    public async Task<ProjectRankingResponse> RankAsync(ProjectRankingRequest request, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();

        var prompt = BuildPrompt(request);
        var result = await CallChatAsync(prompt, ct);
        sw.Stop();

        if (result is null)
        {
            return new ProjectRankingResponse
            {
                ProjectId = request.ProjectId,
                RankedProposals = [],
                Metadata = new RankingMetadata
                {
                    TotalCandidatesEvaluated = request.Candidates.Count,
                    ReturnedCount = 0,
                    ProcessingTime = sw.Elapsed,
                    Warnings = ["AI semantic ranking failed to return a valid response."]
                }
            };
        }

        var ranked = MapToResponse(request, result, sw.Elapsed);
        return ranked;
    }

    private static string BuildPrompt(ProjectRankingRequest request)
    {
        var sb = new StringBuilder();

        sb.AppendLine("=== PROJECT ===");
        sb.AppendLine($"Title: {request.Title}");
        sb.AppendLine($"Description: {request.Description}");
        sb.AppendLine();

        sb.AppendLine("=== CRITERIA ===");
        if (request.Criteria.RequiredSkills?.Count > 0)
            sb.AppendLine($"Required Skills: {string.Join(", ", request.Criteria.RequiredSkills)}");
        if (request.Criteria.PreferredSkills?.Count > 0)
            sb.AppendLine($"Preferred Skills: {string.Join(", ", request.Criteria.PreferredSkills)}");
        if (request.Criteria.BudgetMin.HasValue || request.Criteria.BudgetMax.HasValue)
            sb.AppendLine($"Budget: {request.Criteria.BudgetMin?.ToString("C") ?? "any"} - {request.Criteria.BudgetMax?.ToString("C") ?? "any"}");
        if (!string.IsNullOrEmpty(request.Criteria.ExperienceLevel))
            sb.AppendLine($"Experience Level: {request.Criteria.ExperienceLevel}");
        if (request.Criteria.MinRating.HasValue)
            sb.AppendLine($"Minimum Rating: {request.Criteria.MinRating}");
        if (!string.IsNullOrEmpty(request.Criteria.Timeline))
            sb.AppendLine($"Timeline: {request.Criteria.Timeline}");
        sb.AppendLine();

        sb.AppendLine("=== CANDIDATES ===");
        foreach (var c in request.Candidates)
        {
            sb.AppendLine($"--- Candidate: {c.Name} (ID: {c.Id}) ---");
            if (!string.IsNullOrEmpty(c.Headline)) sb.AppendLine($"Headline: {c.Headline}");
            if (!string.IsNullOrEmpty(c.Bio)) sb.AppendLine($"Bio: {c.Bio}");
            if (c.Skills?.Count > 0)
                sb.AppendLine($"Skills: {string.Join(", ", c.Skills.Select(s => $"{s.Name} ({s.Proficiency})" + (s.YearsExperience.HasValue ? $" [{s.YearsExperience}yr]" : "")))}");
            if (c.Experience is not null)
            {
                sb.AppendLine($"Completed Projects: {c.Experience.CompletedProjects}");
                if (c.Experience.RelevantProjects?.Count > 0)
                    sb.AppendLine($"Relevant Projects: {string.Join(", ", c.Experience.RelevantProjects)}");
                if (!string.IsNullOrEmpty(c.Experience.SpecialtyDomain))
                    sb.AppendLine($"Specialty: {c.Experience.SpecialtyDomain}");
            }
            if (c.Pricing is not null)
                sb.AppendLine($"Pricing: {c.Pricing.HourlyRate?.ToString("C") ?? "?"}/hr | {c.Pricing.FixedPriceEstimate?.ToString("C") ?? "?"} fixed | Negotiable: {c.Pricing.IsOpenToNegotiation}");
            if (c.Reputation is not null)
                sb.AppendLine($"Rating: {c.Reputation.AverageRating?.ToString("F1") ?? "?"} ({c.Reputation.TotalReviews ?? 0} reviews) | Completion: {c.Reputation.CompletionRate?.ToString("P0") ?? "?"} | Verified: {c.Reputation.IsVerified}");
            if (c.PortfolioHighlights?.Count > 0)
                sb.AppendLine($"Portfolio: {string.Join("; ", c.PortfolioHighlights)}");
            if (!string.IsNullOrEmpty(c.MatchContext))
                sb.AppendLine($"Match Context: {c.MatchContext}");
            sb.AppendLine();
        }

        sb.AppendLine("Evaluate each candidate and return the JSON result.");
        return sb.ToString();
    }

    private async Task<ProposalSemanticRankingResult?> CallChatAsync(string prompt, CancellationToken ct)
    {
        var history = new ChatHistory();
        history.AddSystemMessage(PromptTemplates.ProposalRanking);
        history.AddUserMessage(prompt);

        var response = await _chat.GetChatMessageContentsAsync(history, cancellationToken: ct);
        var raw = response.FirstOrDefault()?.Content ?? string.Empty;

        raw = StripCodeFences(raw);

        try
        {
            return JsonSerializer.Deserialize<ProposalSemanticRankingResult>(raw, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }

    private static string StripCodeFences(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.StartsWith("```"))
        {
            var firstNewLine = trimmed.IndexOf('\n');
            if (firstNewLine > 0)
                trimmed = trimmed[(firstNewLine + 1)..];
        }
        if (trimmed.EndsWith("```"))
            trimmed = trimmed[..^3];

        return trimmed.Trim();
    }

    private static ProjectRankingResponse MapToResponse(
        ProjectRankingRequest request,
        ProposalSemanticRankingResult aiResult,
        TimeSpan elapsed)
    {
        var candidateScores = aiResult.Candidates
            .ToDictionary(c => c.CandidateId);

        var ranked = request.Candidates
            .Select(c =>
            {
                candidateScores.TryGetValue(c.Id, out var ai);
                var score = ai?.Score ?? 0.0;
                return new { c, score, ai };
            })
            .OrderByDescending(x => x.score)
            .Select((x, i) => new RankedProposal
            {
                CandidateId = x.c.Id,
                CandidateName = x.c.Name,
                Rank = i + 1,
                OverallScore = x.score,
                ScoreBreakdown = new ScoreBreakdown
                {
                    AiSemanticScore = x.score,
                    WeightedTotal = x.score
                },
                AiReasoning = x.ai?.Reason,
                MatchSummary = new MatchSummary
                {
                    FitVerdict = x.ai?.Summary
                }
            })
            .ToList();

        return new ProjectRankingResponse
        {
            ProjectId = request.ProjectId,
            RankedProposals = ranked,
            AiSummary = aiResult.OverallSummary,
            Metadata = new RankingMetadata
            {
                TotalCandidatesEvaluated = request.Candidates.Count,
                ReturnedCount = ranked.Count,
                ProcessingTime = elapsed,
                UsedAiEmbeddings = false,
                ModelUsed = "meta.llama4-scout-17b-instruct-v1:0"
            }
        };
    }
}
