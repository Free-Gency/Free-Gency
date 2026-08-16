
namespace FreeGency.Application.Features.Plans.Queries;

public class EntitlementService : IEntitlementService
{
    #region Fields
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly IPlanRepository _planRepo;
    private readonly IPlanFeatureRepository _planFeatureRepo;
    private readonly IUsageRecordRepository _usageRecordRepo;
    private readonly IProjectRepository _projectRepo;
    private readonly IProjectProposalRepository _proposalRepo;
    private readonly IProjectInvitationRepository _invitationRepo;
    private readonly ITeamMemberRepository _teamMemberRepo;
    #endregion

    #region Constructor
    public EntitlementService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _subscriptionRepo = unitOfWork.Repository<ISubscriptionRepository, Subscription>();
        _planRepo = unitOfWork.Repository<IPlanRepository, Plan>();
        _planFeatureRepo = unitOfWork.Repository<IPlanFeatureRepository, PlanFeature>();
        _usageRecordRepo = unitOfWork.Repository<IUsageRecordRepository, UsageRecord>();
        _projectRepo = unitOfWork.Repository<IProjectRepository, Project>();
        _proposalRepo = unitOfWork.Repository<IProjectProposalRepository, ProjectProposal>();
        _invitationRepo = unitOfWork.Repository<IProjectInvitationRepository, ProjectInvitation>();
        _teamMemberRepo = unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
    }
    #endregion


    #region Helpers
    private async Task<Subscription> GetOrCreateCurrentSubscriptionAsync(Guid userId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var active = await _subscriptionRepo.GetActiveByUserIdAsync(userId, ct);
        if (active is not null)
            return active;

        var free = await _planRepo.GetFreePlanAsync(ct);
        var existing = await _subscriptionRepo.GetLatestByUserIdAsync(userId, ct);

        if (existing is not null)
        {
            existing.PlanId = free!.Id;
            existing.Status = SubscriptionStatus.Active;
            existing.StartedAt = now;
            existing.ExpiresAt = null;
            existing.AutoRenew = false;
            _subscriptionRepo.Update(existing);
        }
        else
        {
            await _subscriptionRepo.AddAsync(new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PlanId = free!.Id,
                Status = SubscriptionStatus.Active,
                StartedAt = now
            }, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return existing ?? await _subscriptionRepo.GetLatestByUserIdAsync(userId, ct)
            ?? throw new InvalidOperationException("Subscription was not created.");
    }

    private async Task<EntitlementResult> EvaluateAsync(Guid userId, FeatureType feature, bool requireQuota, CancellationToken ct)
    {
        var plan = await GetCurrentPlanAsync(userId, ct);
        var planFeature = (await _planFeatureRepo.GetByPlanIdAsync(plan.Id, ct))
            .FirstOrDefault(f => f.Feature == feature);

        if (planFeature is null)
            return new EntitlementResult(feature, false, false, null, 0, 0, plan.Name,
                $"'{feature}' is not configured for the {plan.Name} plan.");

        if (!planFeature.IsEnabled)
            return new EntitlementResult(feature, false, false, planFeature.Limit, 0, 0, plan.Name,
                $"{feature} is not included in your {plan.Name} plan.");

        if (!planFeature.IsEnabled)
            return new EntitlementResult(feature, false, false, planFeature.Limit, 0, 0, plan.Name,
                $"{feature} is not included in your {plan.Name} plan.");

        // AI / token-based feature: Limit = per-use price, billed from the plan's monthly pool.
        if (AiFeaturePricing.IsTokenBased(feature))
        {
            var price = planFeature.Limit ?? 0;
            var poolUsed = await GetTokensUsedAsync(userId, ct);
            var poolRemaining = Math.Max(plan.AllowedTokens - poolUsed, 0);

            return new EntitlementResult(
                feature,
                !requireQuota || poolRemaining >= price,   // "remaining < limit → don't do it"
                true,
                planFeature.Limit,
                (int)Math.Min(poolUsed, int.MaxValue),
                (int)Math.Min(poolRemaining, int.MaxValue),
                plan.Name);
        }


        if (planFeature.Limit is null)
            return new EntitlementResult(feature, true, true, null, 0, int.MaxValue, plan.Name);

        var used = await GetUsedAsync(userId, feature, ct);
        var remaining = planFeature.Limit.Value - used;

        return new EntitlementResult(
            feature,
            !requireQuota || remaining > 0,   // access ignores quota; consume requires it
            true,
            planFeature.Limit,
            used,
            Math.Max(remaining, 0),
            plan.Name);
    }

    // ---------- used counters (all counting lives in the repos now) ----------
    private async Task<int> GetUsedAsync(Guid userId, FeatureType feature, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        return feature switch
        {
            // LEADER-ONLY: counts the leader's UserId — the person who submits the proposal.
            FeatureType.SendProposal => await _proposalRepo.CountSubmittedByUserSinceAsync(userId, now.Date, ct),

            FeatureType.CreateProject => await _projectRepo.CountCreatedByClientSinceAsync(userId, monthStart, ct),

            FeatureType.ProjectSendInvitation => await _invitationRepo.CountSentByClientSinceAsync(userId, monthStart, ct),

            FeatureType.JoinedTeams => await _teamMemberRepo.CountByUserIdAsync(userId, ct),

            FeatureType.ActiveProjects => await _projectRepo.CountActiveByClientAsync(userId, ct),


            // GenerateProjectDraft, TeamSuggestions, AIChat, HiringAgent → UsageRecord bucket
            _ => await GetUsageRecordUsedAsync(userId, feature, now, ct)
        };
    }

    private async Task<int> GetUsageRecordUsedAsync(Guid userId, FeatureType feature, DateTime now, CancellationToken ct)
    {
        var sub = await _subscriptionRepo.GetLatestByUserIdAsync(userId, ct);
        if (sub is null)
            return 0;

        var record = await _usageRecordRepo.GetBySubscriptionAndFeatureAsync(sub.Id, feature, ct);
        if (record is null || record.PeriodEnd <= now)
            return 0;   // stale bucket (period rolled over) counts as 0

        return record.Used;
    }

    private async Task IncrementUsageAsync(Guid userId, FeatureType feature, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var periodStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var sub = await _subscriptionRepo.GetLatestByUserIdAsync(userId, ct);
        if (sub is null)
            return;

        var record = await _usageRecordRepo.GetBySubscriptionAndFeatureAsync(sub.Id, feature, ct);

        if (record is null)
        {
            await _usageRecordRepo.AddAsync(new UsageRecord
            {
                Id = Guid.NewGuid(),
                SubscriptionId = sub.Id,
                Feature = feature,
                Used = 1,
                PeriodStart = periodStart,
                PeriodEnd = periodStart.AddMonths(1)
            }, ct);
        }
        else if (record.PeriodEnd <= now)
        {
            record.Used = 1;
            record.PeriodStart = periodStart;
            record.PeriodEnd = periodStart.AddMonths(1);
            _usageRecordRepo.Update(record);
        }
        else
        {
            record.Used += 1;
            _usageRecordRepo.Update(record);
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }


    #region Tokens Helpers
    private async Task<long> GetTokensUsedAsync(Guid userId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var sub = await _subscriptionRepo.GetLatestByUserIdAsync(userId, ct);
        if (sub is null)
            return 0;

        long total = 0;
        foreach (var feature in AiFeaturePricing.AiFeatures)
        {
            var record = await _usageRecordRepo.GetBySubscriptionAndFeatureAsync(sub.Id, feature, ct);
            if (record is not null && record.PeriodEnd > now)
                total += record.TokensUsed;
        }
        return total;
    }

    private async Task AddTokensAsync(Guid userId, FeatureType feature, long tokens, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var periodStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var sub = await _subscriptionRepo.GetLatestByUserIdAsync(userId, ct);
        if (sub is null)
            return;

        var record = await _usageRecordRepo.GetBySubscriptionAndFeatureAsync(sub.Id, feature, ct);

        if (record is null)
        {
            await _usageRecordRepo.AddAsync(new UsageRecord
            {
                Id = Guid.NewGuid(),
                SubscriptionId = sub.Id,
                Feature = feature,
                Used = 0,
                TokensUsed = tokens,
                PeriodStart = periodStart,
                PeriodEnd = periodStart.AddMonths(1)
            }, ct);
        }
        else if (record.PeriodEnd <= now)
        {
            record.Used = 0;
            record.TokensUsed = tokens;
            record.PeriodStart = periodStart;
            record.PeriodEnd = periodStart.AddMonths(1);
            _usageRecordRepo.Update(record);
        }
        else
        {
            record.TokensUsed += tokens;
            _usageRecordRepo.Update(record);
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }
    #endregion

    #endregion


    #region Methods
    public async Task<Plan?> GetCurrentPlanAsync(Guid userId, CancellationToken ct = default)
    {
        var subscription = await GetOrCreateCurrentSubscriptionAsync(userId, ct);
        return await _planRepo.GetByIdAsync(subscription.PlanId, ct);
    }

    public async Task<EntitlementResult> CanAccessAsync(Guid userId, FeatureType feature, CancellationToken ct = default)
        => await EvaluateAsync(userId, feature, requireQuota: false, ct);

    public async Task<EntitlementResult> CanConsumeAsync(Guid userId, FeatureType feature, CancellationToken ct = default)
        => await EvaluateAsync(userId, feature, requireQuota: true, ct);

    public async Task<EntitlementResult> ConsumeAsync(Guid userId, FeatureType feature, CancellationToken ct = default)
    {
        var result = await EvaluateAsync(userId, feature, requireQuota: true, ct);
        if (!result.IsAllowed)
            return result;

        if (AiFeaturePricing.IsTokenBased(feature))
        {
            var price = result.Limit ?? 0;
            await AddTokensAsync(userId, feature, price, ct);
            return result with
            {
                Used = (int)(result.Used + price),
                Remaining = (int)(result.Remaining - price)
            };
        }

        await IncrementUsageAsync(userId, feature, ct);
        return result with { Used = result.Used + 1, Remaining = result.Remaining - 1 };
    }

    public async Task<PlanSnapshotDto> GetSnapshotAsync(Guid userId, CancellationToken ct = default)
    {
        var plan = await GetCurrentPlanAsync(userId, ct);
        var features = await _planFeatureRepo.GetByPlanIdAsync(plan.Id, ct);

        var sub = await _subscriptionRepo.GetLatestByUserIdAsync(userId, ct);
        var now = DateTime.UtcNow;

        var usage = new Dictionary<FeatureType, FeatureUsageDto>();
        var tokenUsage = new List<TokenUsageDto>();

        foreach (var f in features)
        {
            if (AiFeaturePricing.IsTokenBased(f.Feature))
            {
                var record = sub is null
                    ? null
                    : await _usageRecordRepo.GetBySubscriptionAndFeatureAsync(sub.Id, f.Feature, ct);
                var used = record is not null && record.PeriodEnd > now ? record.TokensUsed : 0;
                tokenUsage.Add(new TokenUsageDto(f.Feature, used));
                continue;
            }

            var r = await EvaluateAsync(userId, f.Feature, requireQuota: false, ct);
            usage[f.Feature] = new FeatureUsageDto(r.IsEnabled, r.Limit, r.Used, r.Remaining);
        }

        var tokensUsed = tokenUsage.Sum(t => t.TokensUsed);
        var activeSub = await _subscriptionRepo.GetActiveByUserIdAsync(userId, ct);

        return new PlanSnapshotDto(
            plan.Name,
            activeSub is not null,
            activeSub?.ExpiresAt,
            usage,
            tokenUsage,
            plan.AllowedTokens,
            tokensUsed,
            Math.Max(plan.AllowedTokens - tokensUsed, 0));
    }
    #endregion
}