
using FreeGency.Domain.Entities.TeamPlans;
using FreeGency.Domain.Interfaces.Repositories.plansTeam;

namespace FreeGency.Application.Features.PlanTeamFeature.Queries;


public class TeamEntitlementService : ITeamEntitlementService
{
    #region Fields
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITeamSubscriptionRepository _subscriptionRepo;
    private readonly ITeamPlanFeatureRepository _planFeatureRepo;
    private readonly ITeamUsageRecordRepository _usageRecordRepo;
    #endregion

    #region Constructor
    public TeamEntitlementService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _subscriptionRepo = unitOfWork.Repository<ITeamSubscriptionRepository, TeamSubscription>();
        _planFeatureRepo = unitOfWork.Repository<ITeamPlanFeatureRepository, TeamPlanFeature>();
        _usageRecordRepo = unitOfWork.Repository<ITeamUsageRecordRepository, TeamUsageRecord>();
    }
    #endregion

    #region Helpers
    private async Task<TeamSubscription?> GetActiveSubscriptionAsync(Guid teamId, CancellationToken ct)
    {
        var subscription = await _subscriptionRepo.GetByTeamIdAsync(teamId);
        if (subscription is null)
            return null;

        var now = DateTime.UtcNow;
        if (subscription.ExpiresAt is null || subscription.ExpiresAt <= now)
            return null;

        return subscription;
    }

    private async Task<TeamEntitlementResult> EvaluateAsync(Guid teamId, TeamFeatureType feature, bool requireQuota, CancellationToken ct)
    {
        var subscription = await GetActiveSubscriptionAsync(teamId, ct);
        if (subscription is null)
            return new TeamEntitlementResult(feature, false, false, null, 0, 0, "No plan",
                "This team does not have an active team plan. Upgrade to continue.");

        var planFeature = (await _planFeatureRepo.GetByPlanIdAsync(subscription.TeamPlanId))
            .FirstOrDefault(f => f.Feature == feature);

        if (planFeature is null)
            return new TeamEntitlementResult(feature, false, false, null, 0, 0, subscription.TeamPlan.Name,
                $"'{feature}' is not configured for the {subscription.TeamPlan.Name} team plan.");

        if (!planFeature.IsEnabled)
            return new TeamEntitlementResult(feature, false, false, planFeature.Limit, 0, 0, subscription.TeamPlan.Name,
                $"{feature} is not included in your {subscription.TeamPlan.Name} team plan.");

        if (planFeature.Limit is null)
            return new TeamEntitlementResult(feature, true, true, null, 0, int.MaxValue, subscription.TeamPlan.Name);

        var used = await GetUsedAsync(subscription, feature, ct);
        var remaining = planFeature.Limit.Value - used;

        return new TeamEntitlementResult(
            feature,
            !requireQuota || remaining > 0,
            true,
            planFeature.Limit,
            used,
            Math.Max(remaining, 0),
            subscription.TeamPlan.Name);
    }

    private async Task<int> GetUsedAsync(TeamSubscription subscription, TeamFeatureType feature, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var record = (await _usageRecordRepo.GetUsageRecordsBySubscriptionIdAsync(subscription.Id))
            .FirstOrDefault(r => r.Feature == feature);

        if (record is null || record.PeriodEnd <= now)
            return 0;

        return record.Used;
    }

    private async Task IncrementUsageAsync(TeamSubscription subscription, TeamFeatureType feature, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var record = (await _usageRecordRepo.GetUsageRecordsBySubscriptionIdAsync(subscription.Id))
            .FirstOrDefault(r => r.Feature == feature);

        if (record is null)
        {
            await _usageRecordRepo.AddAsync(new TeamUsageRecord
            {
                Id = Guid.NewGuid(),
                TeamSubscriptionId = subscription.Id,
                Feature = feature,
                Used = 1,
                PeriodStart = subscription.StartedAt,
                PeriodEnd = subscription.ExpiresAt!.Value
            }, ct);
        }
        else if (record.PeriodEnd <= now)
        {
            record.Used = 1;
            record.PeriodStart = subscription.StartedAt;
            record.PeriodEnd = subscription.ExpiresAt!.Value;
            _usageRecordRepo.Update(record);
        }
        else
        {
            record.Used += 1;
            _usageRecordRepo.Update(record);
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }
    #endregion

    #region Methods
    public async Task<TeamEntitlementResult> CanAccessTeamFeatureAsync(Guid teamId, TeamFeatureType feature, CancellationToken ct = default)
        => await EvaluateAsync(teamId, feature, requireQuota: false, ct);

    public async Task<TeamEntitlementResult> CanConsumeTeamFeatureAsync(Guid teamId, TeamFeatureType feature, CancellationToken ct = default)
        => await EvaluateAsync(teamId, feature, requireQuota: true, ct);

    public async Task<TeamEntitlementResult> ConsumeTeamFeatureAsync(Guid teamId, TeamFeatureType feature, CancellationToken ct = default)
    {
        var subscription = await GetActiveSubscriptionAsync(teamId, ct);
        if (subscription is null)
            return await EvaluateAsync(teamId, feature, requireQuota: true, ct);

        var result = await EvaluateAsync(teamId, feature, requireQuota: true, ct);
        if (!result.IsAllowed)
            return result;

        await IncrementUsageAsync(subscription, feature, ct);
        return result with { Used = result.Used + 1, Remaining = result.Remaining - 1 };
    }
    #endregion
}
