using System.Diagnostics;
using FreeGency.AI.DTOs;
using FreeGency.AI.Interfaces;
using FreeGency.AI.Ranking.ProposalRanking;

namespace FreeGency.AI.Services;

public sealed class AIOrchestrator : IAIOrchestrator
{
    private readonly IProposalRuleEngine _ruleEngine;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStore _vectorStore;
    private readonly IProposalRankingService _semanticRanking;
    private readonly IAICacheService _cache;

    private const string CollectionName = "proposals";
    private const int VectorSearchTopK = 10;

    public AIOrchestrator(
        IProposalRuleEngine ruleEngine,
        IEmbeddingService embeddingService,
        IVectorStore vectorStore,
        IProposalRankingService semanticRanking,
        IAICacheService cache)
    {
        _ruleEngine = ruleEngine;
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
        _semanticRanking = semanticRanking;
        _cache = cache;
    }

    public async Task<ProjectRankingResponse> RankProjectAsync(ProjectRankingRequest request, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var warnings = new List<string>();

        // ── 1. Cache Check ──────────────────────────────────────────────
        var cacheKey = $"ai:ranking:{request.ProjectId}";
        var cached = await _cache.GetAsync<ProjectRankingResponse>(cacheKey, ct);
        if (cached is not null)
        {
            sw.Stop();
            return new ProjectRankingResponse
            {
                ProjectId = cached.ProjectId,
                RankedProposals = cached.RankedProposals,
                AiSummary = cached.AiSummary,
                Metadata = new RankingMetadata
                {
                    TotalCandidatesEvaluated = cached.Metadata.TotalCandidatesEvaluated,
                    ReturnedCount = cached.Metadata.ReturnedCount,
                    ProcessingTime = sw.Elapsed,
                    UsedAiEmbeddings = cached.Metadata.UsedAiEmbeddings,
                    FromCache = true,
                    ModelUsed = cached.Metadata.ModelUsed,
                    Warnings = cached.Metadata.Warnings
                }
            };
        }

        // ── 2. Rule Engine → TopK candidates ────────────────────────────
        var ruleTake = Math.Clamp(request.TopK > 0 ? request.TopK : 20, 1, 100);
        var ruleResult = _ruleEngine.Rank(request);
        var ruleTop = ruleResult.RankedProposals
            .OrderByDescending(p => p.OverallScore)
            .Take(ruleTake)
            .ToList();

        var ruleCandidateIds = new HashSet<string>(ruleTop.Select(p => p.CandidateId));
        var candidateMap = request.Candidates.ToDictionary(c => c.Id);

        // ── 3. Embed Project Description ────────────────────────────────
        float[] projectEmbedding = [];
        try
        {
            projectEmbedding = await _embeddingService.EmbedProjectDescriptionAsync(
                $"{request.Title} {request.Description}", ct);
        }
        catch
        {
            // Embedding service unavailable — proceed with rule-based + semantic only
        }

        // ── 4. Chroma Vector Search ─────────────────────────────────────
        var vectorCandidateIds = new HashSet<string>();
        if (projectEmbedding.Length > 0)
        {
            try
            {
                var searchResults = await _vectorStore.SearchAsync(
                    CollectionName, projectEmbedding, topK: VectorSearchTopK, ct: ct);

                foreach (var result in searchResults)
                {
                    if (candidateMap.ContainsKey(result.Id))
                        vectorCandidateIds.Add(result.Id);
                }
            }
            catch
            {
                // Vector store unavailable — use rule-based candidates only
            }
        }

        // ── 5. Merge Candidates (union, dedup) ──────────────────────────
        var mergedCandidateIds = new HashSet<string>(ruleCandidateIds);
        mergedCandidateIds.UnionWith(vectorCandidateIds);

        var mergedCandidates = mergedCandidateIds
            .Where(id => candidateMap.ContainsKey(id))
            .Select(id => candidateMap[id])
            .ToList();

        if (mergedCandidates.Count == 0)
        {
            sw.Stop();
            return new ProjectRankingResponse
            {
                ProjectId = request.ProjectId,
                RankedProposals = [],
                Metadata = new RankingMetadata
                {
                    TotalCandidatesEvaluated = request.Candidates.Count,
                    ReturnedCount = 0,
                    ProcessingTime = sw.Elapsed,
                    UsedAiEmbeddings = projectEmbedding.Length > 0,
                    Warnings = ["No candidates matched after rule engine and vector search."]
                }
            };
        }

        // ── 6. Semantic Ranking (LLM) ───────────────────────────────────
        var semanticRequest = new ProjectRankingRequest
        {
            ProjectId = request.ProjectId,
            Title = request.Title,
            Description = request.Description,
            Criteria = request.Criteria,
            Candidates = mergedCandidates,
            TopK = request.TopK
        };

        ProjectRankingResponse semanticResult;
        try
        {
            semanticResult = await _semanticRanking.RankAsync(semanticRequest, ct);
        }
        catch (Exception ex)
        {
            warnings.Add($"Semantic ranking failed: {ex.Message}. Falling back to rule-based ranking.");
            sw.Stop();
            return new ProjectRankingResponse
            {
                ProjectId = request.ProjectId,
                RankedProposals = ruleTop,
                AiSummary = null,
                Metadata = new RankingMetadata
                {
                    TotalCandidatesEvaluated = request.Candidates.Count,
                    ReturnedCount = ruleTop.Count,
                    ProcessingTime = sw.Elapsed,
                    UsedAiEmbeddings = false,
                    ModelUsed = null,
                    Warnings = warnings
                }
            };
        }

        // ── 7. Merge Scores ─────────────────────────────────────────────
        var ruleResultMap = ruleTop.ToDictionary(p => p.CandidateId, p => p);
        var finalProposals = MergeScores(semanticResult.RankedProposals, ruleResultMap, request.TopK);

        // ── 8. Cache Result ─────────────────────────────────────────────
        var response = new ProjectRankingResponse
        {
            ProjectId = request.ProjectId,
            RankedProposals = finalProposals,
            AiSummary = semanticResult.AiSummary,
            Metadata = new RankingMetadata
            {
                TotalCandidatesEvaluated = request.Candidates.Count,
                ReturnedCount = finalProposals.Count,
                ProcessingTime = sw.Elapsed,
                UsedAiEmbeddings = projectEmbedding.Length > 0,
                ModelUsed = "google.gemma-3-27b-it",
                Warnings = warnings.Count > 0 ? warnings : null
            }
        };

        await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(30), ct);

        return response;
    }

    private static IReadOnlyList<RankedProposal> MergeScores(
        IReadOnlyList<RankedProposal> semanticProposals,
        Dictionary<string, RankedProposal> ruleResults,
        int topK)
    {
        return semanticProposals
            .Select(p =>
            {
                ruleResults.TryGetValue(p.CandidateId, out var rule);
                var ruleScore = rule?.OverallScore ?? 0.0;
                var semanticScore = p.OverallScore * 100.0;
                var combined = (semanticScore * 0.6) + (ruleScore * 0.4);

                var ruleBreakdown = rule?.ScoreBreakdown;
                var ruleMatch = rule?.MatchSummary;

                return new RankedProposal
                {
                    CandidateId = p.CandidateId,
                    CandidateName = p.CandidateName,
                    Rank = 0,
                    OverallScore = Math.Round(combined, 2),
                    ScoreBreakdown = new ScoreBreakdown
                    {
                        SkillMatch = Math.Round((ruleBreakdown?.SkillMatch ?? p.ScoreBreakdown.SkillMatch) * 100.0, 2),
                        ExperienceRelevance = Math.Round((ruleBreakdown?.ExperienceRelevance ?? p.ScoreBreakdown.ExperienceRelevance) * 100.0, 2),
                        ReputationScore = Math.Round((ruleBreakdown?.ReputationScore ?? p.ScoreBreakdown.ReputationScore) * 100.0, 2),
                        BudgetFit = Math.Round((ruleBreakdown?.BudgetFit ?? p.ScoreBreakdown.BudgetFit) * 100.0, 2),
                        AvailabilityFit = Math.Round((ruleBreakdown?.AvailabilityFit ?? p.ScoreBreakdown.AvailabilityFit) * 100.0, 2),
                        ProposalQuality = Math.Round((ruleBreakdown?.ProposalQuality ?? p.ScoreBreakdown.ProposalQuality) * 100.0, 2),
                        AiSemanticScore = Math.Round(semanticScore, 2),
                        WeightedTotal = Math.Round(combined, 2)
                    },
                    AiReasoning = string.IsNullOrEmpty(p.AiReasoning) ? rule?.AiReasoning : p.AiReasoning,
                    MatchSummary = new MatchSummary
                    {
                        MatchedRequiredSkills = ruleMatch?.MatchedRequiredSkills ?? p.MatchSummary?.MatchedRequiredSkills ?? 0,
                        TotalRequiredSkills = ruleMatch?.TotalRequiredSkills ?? p.MatchSummary?.TotalRequiredSkills ?? 0,
                        MatchedPreferredSkills = ruleMatch?.MatchedPreferredSkills ?? p.MatchSummary?.MatchedPreferredSkills ?? 0,
                        TotalPreferredSkills = ruleMatch?.TotalPreferredSkills ?? p.MatchSummary?.TotalPreferredSkills ?? 0,
                        MissingSkills = ruleMatch?.MissingSkills ?? p.MatchSummary?.MissingSkills,
                        FitVerdict = p.MatchSummary?.FitVerdict ?? ruleMatch?.FitVerdict
                    }
                };
            })
            .OrderByDescending(p => p.OverallScore)
            .Select((p, i) => new RankedProposal
            {
                CandidateId = p.CandidateId,
                CandidateName = p.CandidateName,
                Rank = i + 1,
                OverallScore = p.OverallScore,
                ScoreBreakdown = p.ScoreBreakdown,
                AiReasoning = p.AiReasoning,
                MatchSummary = p.MatchSummary
            })
            .Take(topK)
            .ToList();
    }

    public Task<AIResponse<T>> ProcessAsync<T>(AIRequest request, CancellationToken ct = default) where T : class
    {
        return Task.FromResult(AIResponse<T>.Failure("Use RankProjectAsync for proposal ranking."));
    }

    public Task<AIResponse<string>> ChatAsync(string prompt, IDictionary<string, string>? context = null, CancellationToken ct = default)
    {
        return Task.FromResult(AIResponse<string>.Failure("Use RankProjectAsync for proposal ranking."));
    }
}
