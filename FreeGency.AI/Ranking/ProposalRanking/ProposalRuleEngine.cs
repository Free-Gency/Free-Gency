namespace FreeGency.AI.Ranking.ProposalRanking;

public sealed class ProposalRuleEngine : IProposalRuleEngine
{
    private const int MaxReturnCount = 20;

    private static readonly ScoringWeights DefaultWeights = new();
    private readonly ScoringWeights _weights;

    public ProposalRuleEngine() : this(DefaultWeights) { }

    public ProposalRuleEngine(ScoringWeights weights)
    {
        _weights = weights;
    }

    public ProjectRankingResponse Rank(ProjectRankingRequest request)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var warnings = new List<string>();

        if (request.Candidates.Count == 0)
        {
            sw.Stop();
            return BuildEmptyResponse(request, sw.Elapsed, warnings);
        }

        var requiredSet = ToSet(request.Criteria.RequiredSkills);
        var preferredSet = ToSet(request.Criteria.PreferredSkills);

        if (requiredSet.Count == 0 && preferredSet.Count == 0)
            warnings.Add("No required or preferred skills specified. Skill match score will be zero for all candidates.");

        var scored = new List<RankedProposal>(request.Candidates.Count);

        foreach (var candidate in request.Candidates)
        {
            var skillScore = ScoreSkillMatch(candidate, requiredSet, preferredSet);
            var budgetScore = ScoreBudgetMatch(candidate, request.Criteria);
            var projectsScore = ScoreCompletedProjects(candidate, request.Criteria);
            var ratingScore = ScoreRating(candidate, request.Criteria);
            var qualityScore = ScoreProposalQuality(candidate);

            var weightedTotal =
                skillScore * _weights.SkillMatch +
                budgetScore * _weights.BudgetMatch +
                projectsScore * _weights.CompletedProjects +
                ratingScore * _weights.Rating +
                qualityScore * _weights.ProposalQuality;

            var breakdown = new ScoreBreakdown
            {
                SkillMatch = Math.Round(skillScore, 4),
                BudgetFit = Math.Round(budgetScore, 4),
                ExperienceRelevance = Math.Round(projectsScore, 4),
                ReputationScore = Math.Round(ratingScore, 4),
                ProposalQuality = Math.Round(qualityScore, 4),
                AvailabilityFit = 0,
                AiSemanticScore = 0,
                WeightedTotal = Math.Round(weightedTotal, 4)
            };

            var matchSummary = BuildMatchSummary(candidate, requiredSet, preferredSet);

            scored.Add(new RankedProposal
            {
                CandidateId = candidate.Id,
                CandidateName = candidate.Name,
                Rank = 0,
                OverallScore = Math.Round(weightedTotal, 4),
                ScoreBreakdown = breakdown,
                MatchSummary = matchSummary
            });
        }

        var ranked = scored
            .OrderByDescending(x => x.OverallScore)
            .ThenByDescending(x => x.ScoreBreakdown.SkillMatch)
            .ThenByDescending(x => x.ScoreBreakdown.ReputationScore)
            .Take(Math.Min(request.TopK, MaxReturnCount))
            .Select((x, i) => new RankedProposal
            {
                CandidateId = x.CandidateId,
                CandidateName = x.CandidateName,
                Rank = i + 1,
                OverallScore = x.OverallScore,
                ScoreBreakdown = x.ScoreBreakdown,
                AiReasoning = BuildReasoning(x),
                MatchSummary = x.MatchSummary
            })
            .ToList();

        sw.Stop();

        return new ProjectRankingResponse
        {
            ProjectId = request.ProjectId,
            RankedProposals = ranked,
            Metadata = new RankingMetadata
            {
                TotalCandidatesEvaluated = request.Candidates.Count,
                ReturnedCount = ranked.Count,
                ProcessingTime = sw.Elapsed,
                UsedAiEmbeddings = false,
                FromCache = false,
                Warnings = warnings.Count > 0 ? warnings : null
            }
        };
    }

    // ── Skill Match (0..1) ─────────────────────────────────────────────
    private static double ScoreSkillMatch(
        ProposalCandidate candidate,
        HashSet<string> requiredSet,
        HashSet<string> preferredSet)
    {
        if (requiredSet.Count == 0 && preferredSet.Count == 0)
            return 0;

        var candidateSkills = ToSkillMap(candidate.Skills);
        double score = 0;

        // Required skills: each missing costs heavily
        if (requiredSet.Count > 0)
        {
            int matched = 0;
            double proficiencySum = 0;

            foreach (var skill in requiredSet)
            {
                if (candidateSkills.TryGetValue(skill, out var prof)
                    || candidateSkills.TryGetValue(Normalize(skill), out prof))
                {
                    matched++;
                    proficiencySum += ProficiencyToDouble(prof);
                }
            }

            var coverage = (double)matched / requiredSet.Count;
            var avgProficiency = matched > 0 ? proficiencySum / matched : 0;
            score = coverage * 0.7 + avgProficiency * 0.3;
        }

        // Preferred skills: bonus up to 0.3 of the score
        if (preferredSet.Count > 0)
        {
            int matchedPreferred = 0;
            foreach (var skill in preferredSet)
            {
                if (candidateSkills.ContainsKey(skill) || candidateSkills.ContainsKey(Normalize(skill)))
                    matchedPreferred++;
            }

            var preferredBonus = (double)matchedPreferred / preferredSet.Count * 0.3;
            score = Math.Min(1.0, score + preferredBonus);
        }

        return Math.Clamp(score, 0, 1);
    }

    // ── Budget Match (0..1) ────────────────────────────────────────────
    private static double ScoreBudgetMatch(ProposalCandidate candidate, RankingCriteria criteria)
    {
        var rate = candidate.Pricing?.HourlyRate ?? candidate.Pricing?.FixedPriceEstimate;
        if (rate is null || (!criteria.BudgetMin.HasValue && !criteria.BudgetMax.HasValue))
            return 0.5; // neutral when no data

        var min = criteria.BudgetMin ?? 0;
        var max = criteria.BudgetMax ?? decimal.MaxValue;

        if (min > max)
            return 0;

        if (rate.Value >= min && rate.Value <= max)
            return 1.0;

        // Outside range: score drops linearly based on distance
        decimal distance;
        decimal range = max - min == 0 ? 1 : max - min;

        if (rate.Value < min)
            distance = min - rate.Value;
        else
            distance = rate.Value - max;

        var penalty = (double)(distance / range);
        return Math.Max(0, 1.0 - penalty);
    }

    // ── Completed Projects (0..1) ──────────────────────────────────────
    private static double ScoreCompletedProjects(ProposalCandidate candidate, RankingCriteria criteria)
    {
        var completed = candidate.Experience?.CompletedProjects ?? 0;
        var threshold = criteria.MinCompletedProjects ?? 10;

        if (threshold <= 0) threshold = 10;

        // Logarithmic scale: 0 at 0, ~0.5 at threshold, approaches 1.0 above
        if (completed == 0) return 0;

        var ratio = (double)completed / threshold;
        return Math.Clamp(Math.Log2(1 + ratio), 0, 1);
    }

    // ── Rating (0..1) ──────────────────────────────────────────────────
    private static double ScoreRating(ProposalCandidate candidate, RankingCriteria criteria)
    {
        var rating = candidate.Reputation?.AverageRating;
        if (rating is null) return 0.5; // neutral

        var minRating = criteria.MinRating ?? 0;

        // Scale: minRating maps to 0, 5.0 maps to 1.0
        var effectiveMin = Math.Max(minRating, 0);
        var range = 5.0 - effectiveMin;
        if (range <= 0) return 1.0;

        return Math.Clamp((rating.Value - effectiveMin) / range, 0, 1);
    }

    // ── Proposal Quality (0..1) ────────────────────────────────────────
    private static double ScoreProposalQuality(ProposalCandidate candidate)
    {
        double score = 0;
        int factors = 0;

        if (!string.IsNullOrWhiteSpace(candidate.Headline)) { score += 0.25; factors++; }
        if (!string.IsNullOrWhiteSpace(candidate.Bio) && candidate.Bio.Length >= 50) { score += 0.30; factors++; }
        if (candidate.PortfolioHighlights is { Count: > 0 }) { score += 0.25; factors++; }
        if (candidate.Reputation?.IsVerified == true) { score += 0.10; factors++; }
        if (candidate.Reputation?.CompletionRate is >= 0.9) { score += 0.10; factors++; }

        return Math.Clamp(score, 0, 1);
    }

    // ── Helpers ─────────────────────────────────────────────────────────
    private static MatchSummary BuildMatchSummary(
        ProposalCandidate candidate,
        HashSet<string> requiredSet,
        HashSet<string> preferredSet)
    {
        var candidateSkills = ToSkillMap(candidate.Skills);
        var matchedRequired = new List<string>();
        var missingRequired = new List<string>();

        foreach (var skill in requiredSet)
        {
            if (candidateSkills.ContainsKey(skill) || candidateSkills.ContainsKey(Normalize(skill)))
                matchedRequired.Add(skill);
            else
                missingRequired.Add(skill);
        }

        int matchedPreferred = 0;
        foreach (var skill in preferredSet)
        {
            if (candidateSkills.ContainsKey(skill) || candidateSkills.ContainsKey(Normalize(skill)))
                matchedPreferred++;
        }

        return new MatchSummary
        {
            MatchedRequiredSkills = matchedRequired.Count,
            TotalRequiredSkills = requiredSet.Count,
            MatchedPreferredSkills = matchedPreferred,
            TotalPreferredSkills = preferredSet.Count,
            MissingSkills = missingRequired.Count > 0 ? missingRequired : null,
            FitVerdict = BuildFitVerdict(matchedRequired.Count, requiredSet.Count, matchedPreferred, preferredSet.Count)
        };
    }

    private static string BuildFitVerdict(int matchedReq, int totalReq, int matchedPref, int totalPref)
    {
        if (totalReq == 0 && totalPref == 0) return "No skill requirements specified.";

        var reqRatio = totalReq > 0 ? (double)matchedReq / totalReq : 1.0;
        var prefRatio = totalPref > 0 ? (double)matchedPref / totalPref : 1.0;

        return reqRatio switch
        {
            1.0 when prefRatio >= 0.5 => "Excellent fit — all required skills matched.",
            1.0 => "Strong fit — all required skills matched.",
            >= 0.7 => "Good fit — most required skills matched.",
            >= 0.4 => "Partial fit — some required skills missing.",
            _ => "Poor fit — significant skill gaps."
        };
    }

    private static string BuildReasoning(RankedProposal proposal)
    {
        var parts = new List<string>();

        if (proposal.ScoreBreakdown.SkillMatch >= 0.7)
            parts.Add($"Strong skill match ({proposal.ScoreBreakdown.SkillMatch:P0})");
        else if (proposal.ScoreBreakdown.SkillMatch < 0.3)
            parts.Add($"Weak skill match ({proposal.ScoreBreakdown.SkillMatch:P0})");

        if (proposal.ScoreBreakdown.BudgetFit >= 0.9)
            parts.Add("within budget");
        else if (proposal.ScoreBreakdown.BudgetFit < 0.5)
            parts.Add("budget concern");

        if (proposal.ScoreBreakdown.ReputationScore >= 0.8)
            parts.Add("highly rated");

        if (proposal.MatchSummary?.MissingSkills is { Count: > 0 } missing)
            parts.Add($"missing: {string.Join(", ", missing.Take(3))}");

        return parts.Count > 0 ? string.Join(". ", parts) + "." : string.Empty;
    }

    private static HashSet<string> ToSet(IReadOnlyList<string>? items)
    {
        if (items is null or { Count: 0 })
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        return new HashSet<string>(items.Where(s => !string.IsNullOrWhiteSpace(s)).Select(Normalize), StringComparer.OrdinalIgnoreCase);
    }

    private static Dictionary<string, SkillProficiency> ToSkillMap(IReadOnlyList<CandidateSkill>? skills)
    {
        var map = new Dictionary<string, SkillProficiency>(StringComparer.OrdinalIgnoreCase);
        if (skills is null) return map;

        foreach (var skill in skills)
        {
            if (!string.IsNullOrWhiteSpace(skill.Name))
                map[Normalize(skill.Name)] = skill.Proficiency;
        }

        return map;
    }

    private static string Normalize(string s) => s.Trim().ToLowerInvariant();

    private static double ProficiencyToDouble(SkillProficiency p) => p switch
    {
        SkillProficiency.Beginner => 0.25,
        SkillProficiency.Intermediate => 0.50,
        SkillProficiency.Advanced => 0.75,
        SkillProficiency.Expert => 1.0,
        _ => 0.5
    };

    private static ProjectRankingResponse BuildEmptyResponse(
        ProjectRankingRequest request,
        TimeSpan elapsed,
        List<string> warnings)
    {
        warnings.Add("No candidates provided to rank.");
        return new ProjectRankingResponse
        {
            ProjectId = request.ProjectId,
            RankedProposals = [],
            Metadata = new RankingMetadata
            {
                TotalCandidatesEvaluated = 0,
                ReturnedCount = 0,
                ProcessingTime = elapsed,
                UsedAiEmbeddings = false,
                FromCache = false,
                Warnings = warnings
            }
        };
    }
}
