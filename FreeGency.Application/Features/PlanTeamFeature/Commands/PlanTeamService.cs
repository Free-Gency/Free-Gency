using FreeGency.Application.Features.PlanTeamFeature.Dtos;
using FreeGency.Domain.Entities.TeamPlans;
using FreeGency.Domain.Interfaces.Repositories.plansTeam;
using FreeGency.Infrastructure.Persistence.Repositories.PlansTeam;
using Stripe;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.PlanTeamFeature.Commands
{
    public partial class PlanTeamService : IPlanTeamService
    {
        private readonly IWalletRepository walletRepository = unitOfWork.Repository<IWalletRepository, Wallet>();
        private readonly ITeamMemberRepository teamMemberRepository = unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
        private readonly ITeamPlanFeatureRepository teamPlanFeatureRepository = unitOfWork.Repository<ITeamPlanFeatureRepository, TeamPlanFeature>();
        public async Task<Result> ChangeMyPlan(ChangeTeamPlanDto dto)
        {
            var isLeader = await teamMemberRepository.IsLeaderAsync(
                dto.TeamId,
                currentUserService.UserId);

            if (!isLeader)
                return Result.Failure(TeamPlanErrors.NotTeamLeader);

            var subscription = await teamSubscriptionRepository
                .GetByIdAsync(dto.SubscriptionId);

            if (subscription is null)
                return Result.Failure(TeamPlanErrors.SubscriptionNotFound);

            if (subscription.TeamId != dto.TeamId)
                return Result.Failure(TeamPlanErrors.SubscriptionNotFound);

            var newPlan = await teamPlanRepository.GetByIdAsync(dto.PlanId);

            if (newPlan is null)
                return Result.Failure(TeamPlanErrors.PlanNotFound);

            var price = dto.BillingPeriod == BillingPeriod.Monthly
                ? newPlan.MonthlyPrice
                : newPlan.YearlyPrice;

            if (price is null)
                return Result.Failure(TeamPlanErrors.InvalidPlanPrice);

            var wallet = await walletRepository.GetByOwnerAsync(
                owner.User,
                currentUserService.UserId);

            if (wallet is null)
                return Result.Failure(WalletErrors.NotFound);

            if (wallet.Available < price.Value)
                return Result.Failure(TeamPlanErrors.InsufficientBalance);

            wallet.Available -= price.Value;

            var now = DateTime.UtcNow;

            subscription.TeamPlanId = newPlan.Id;
            subscription.BillingPeriod = dto.BillingPeriod;
            subscription.StartedAt = now;
            subscription.UserId = currentUserService.UserId;
            subscription.ExpiresAt =
                dto.BillingPeriod == BillingPeriod.Monthly
                    ? now.AddMonths(1)
                    : now.AddYears(1);

            await SyncUsageWithPlanAsync(subscription, newPlan.Id);

            await unitOfWork.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<SubscriptionDto?> GetMySubscription(Guid teamId)
        {
            var subscription =
                await teamSubscriptionRepository.GetByTeamIdAsync(teamId);

            if (subscription is null)
                return null;

            var price = subscription.BillingPeriod == BillingPeriod.Monthly
                ? subscription.TeamPlan.MonthlyPrice
                : subscription.TeamPlan.YearlyPrice;

            var features = await teamPlanFeatureRepository
                .GetByPlanIdAsync(subscription.TeamPlanId);

            return new SubscriptionDto
            {
                Id = subscription.Id,

                TeamId = subscription.TeamId,

                TeamPlanId = subscription.TeamPlanId,

                PlanName = subscription.TeamPlan.Name,

                BillingPeriod = subscription.BillingPeriod,

                AutoRenew = subscription.AutoRenew,

                StartedAt = subscription.StartedAt,

                ExpiresAt = subscription.ExpiresAt,

                Price = price ?? 0,

                Features = features
                   
                    .Select(x => new PlanFeatuerTeamDto
                    {
                        Feature = x.Feature.ToString(),
                        Limit = x.Limit,
                        IsEnabled = x.IsEnabled
                    })
                    .ToList()
            };
        }

        public async Task<Result<bool>> ToggleAutoRenew(Guid SubId)
        {
            var subscription =
                 await teamSubscriptionRepository.GetByIdAsync(SubId);
            if (subscription is null)
            {
                return Result.Failure<bool>(
                    TeamPlanErrors.SubscriptionNotFound);
            }

            var isLeader = await teamMemberRepository.IsLeaderAsync(
                subscription.TeamId,
                currentUserService.UserId);

            if (!isLeader)
            {
                return Result.Failure<bool>(
                    TeamPlanErrors.NotTeamLeader);
            }

            subscription.AutoRenew = !subscription.AutoRenew;

            await unitOfWork.SaveChangesAsync();

            return Result.Success(subscription.AutoRenew);
        }
        private async Task SyncUsageWithPlanAsync(
     TeamSubscription subscription,
     Guid planId)
        {
            var usageRecords =
                await teamUsageRecordRepository
                    .GetUsageRecordsBySubscriptionIdAsync(subscription.Id);

            var planFeatures =
                await teamPlanFeatureRepository
                    .GetByPlanIdAsync(planId);

            var activeFeatures = planFeatures
                .Where(x => x.IsEnabled)
                .Select(x => x.Feature)
                .ToHashSet();

            // نحذف الـ Usage Records للـ Features
            // اللي مش موجودة في الـ Plan الجديدة
            foreach (var usage in usageRecords)
            {
                if (!activeFeatures.Contains(usage.Feature))
                {
                    teamUsageRecordRepository.Delete(usage);
                    continue;
                }

                usage.Used = 0;
                usage.PeriodStart = subscription.StartedAt;
                usage.PeriodEnd = subscription.ExpiresAt!.Value;
            }

            // الـ Usage الموجودة فعلاً
            var existingFeatures = usageRecords
                .Where(x => activeFeatures.Contains(x.Feature))
                .Select(x => x.Feature)
                .ToHashSet();

            // نضيف الـ Features الجديدة
            foreach (var feature in planFeatures.Where(x => x.IsEnabled))
            {
                if (existingFeatures.Contains(feature.Feature))
                    continue;

                await teamUsageRecordRepository.AddAsync(
                    new TeamUsageRecord
                    {
                        Id = Guid.NewGuid(),
                        TeamSubscriptionId = subscription.Id,
                        Feature = feature.Feature,
                        Used = 0,
                        PeriodStart = subscription.StartedAt,
                        PeriodEnd = subscription.ExpiresAt!.Value
                    });
            }
        }
    }
}
