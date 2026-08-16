using FreeGency.Application.Common.Errors;
using FreeGency.Application.Features.Plans.Dtos;
using FreeGency.Application.Features.Plans.Mapping;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Entities.Plans;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Domain.Interfaces.Repositories.Plans;
using FreeGency.Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Application.Features.Plans.Commands;

public partial class PlanService
{
    private readonly ISubscriptionRepository subscriptionRepository = unitOfWork.Repository<ISubscriptionRepository, Subscription>();
    private readonly IUsageRecordRepository usageRecordRepository = unitOfWork.Repository<IUsageRecordRepository, UsageRecord>();
    private readonly IWalletRepository walletRepository = unitOfWork.Repository<IWalletRepository, Wallet>();
    private readonly ILedgerEntryRepository ledgerEntryRepository = unitOfWork.Repository<ILedgerEntryRepository, LedgerEntry>();

    private static readonly FeatureType[] UsageBucketFeatures =
    [
        FeatureType.GenerateProjectDraft,
        FeatureType.TeamSuggestions,
        FeatureType.AIChatProposal,
        FeatureType.ProposalRanking,
        FeatureType.HiringAgent
    ];

    public async Task<Result<PlanDto>> ChangeSubscriptionAsync(
        Guid userId, ChangePlanRequestDto request, CancellationToken ct = default)
    {
        var newPlan = await planRepository.GetPlanFeatuerId(request.PlanId)
            .Include(p => p.Features)
            .FirstOrDefaultAsync(ct);

        if (newPlan is null || !newPlan.IsActive)
            return Result.Failure<PlanDto>(PlanErrors.NotFound);

        var price = request.BillingPeriod == BillingPeriod.Yearly
            ? newPlan.YearlyPrice
            : newPlan.MonthlyPrice;

        var wallet = await walletRepository.GetEntityWithSpec(new WalletSpecification(userId));
        if (wallet is null)
            return Result.Failure<PlanDto>(WalletErrors.NotFound);

        if (wallet.Available < price)
            return Result.Failure<PlanDto>(WalletErrors.InsufficientBalance);

        var now = DateTime.UtcNow;
        var expiresAt = request.BillingPeriod == BillingPeriod.Yearly
            ? now.AddYears(1)
            : now.AddMonths(1);

        var existingSubscription = await subscriptionRepository.GetLatestByUserIdAsync(userId, ct);

        if (existingSubscription is not null
            && existingSubscription.PlanId == newPlan.Id
            && existingSubscription.Status == SubscriptionStatus.Active
            && (existingSubscription.ExpiresAt is null || existingSubscription.ExpiresAt > DateTime.UtcNow))
        {
            return Result.Failure<PlanDto>(PlanErrors.AlreadySubscribed);
        }

        var subscription = existingSubscription;
        if (subscription is null)
        {
            subscription = new Subscription { Id = Guid.NewGuid(), UserId = userId };
            await subscriptionRepository.AddAsync(subscription, ct);
        }

        subscription.PlanId = newPlan.Id;
        subscription.Status = SubscriptionStatus.Active;
        subscription.BillingPeriod = request.BillingPeriod;
        subscription.StartedAt = now;
        subscription.ExpiresAt = expiresAt;
        subscription.AutoRenew = true;
        subscriptionRepository.Update(subscription);

        if (price > 0)
        {
            wallet.Available -= price;

            var ledger = new LedgerEntry
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                EntryType = EntryType.PlatformFee,
                Amount = -price,
                Currency = wallet.Currency,
                IdempotencyKey = $"plan-{subscription.Id}-{now:yyyyMMddHHmmss}",
                CreatedAt = now
            };
            await ledgerEntryRepository.AddAsync(ledger, ct);
        }

        await ResetUsageAsync(subscription.Id, now, ct);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(newPlan.ToDto());
    }

    private async Task ResetUsageAsync(Guid subscriptionId, DateTime now, CancellationToken ct)
    {
        var periodStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = periodStart.AddMonths(1);

        var existing = (await usageRecordRepository.GetUsageRecordsBySubId(subscriptionId))
            .ToDictionary(r => r.Feature);

        foreach (var feature in UsageBucketFeatures)
        {
            if (existing.TryGetValue(feature, out var record))
            {
                record.Used = 0;
                record.PeriodStart = periodStart;
                record.PeriodEnd = periodEnd;
                usageRecordRepository.Update(record);
            }
            else
            {
                await usageRecordRepository.AddAsync(new UsageRecord
                {
                    Id = Guid.NewGuid(),
                    SubscriptionId = subscriptionId,
                    Feature = feature,
                    Used = 0,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd
                }, ct);
            }
        }
    }
}
