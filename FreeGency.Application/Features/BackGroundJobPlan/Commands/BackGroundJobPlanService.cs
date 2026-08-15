using FreeGency.Domain.Entities.Plans;
using FreeGency.Domain.Interfaces.Repositories.Plans;
using FreeGency.Infrastructure.Persistence.Seeding;
using Stripe;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.BackGroundJobPlan.Commands
{
    public class BackGroundJobPlanService(IUnitOfWork unitOfWork) : IBackGroundJobPlanService
    {
        private readonly ISubscriptionRepository subscriptionRepository = unitOfWork.Repository<ISubscriptionRepository, Domain.Entities.Plans.Subscription>();
        private readonly IUsageRecordRepository usageRecordRepository = unitOfWork.Repository<IUsageRecordRepository, UsageRecord>();
        private readonly IPlanFeatureRepository planFeatureRepository = unitOfWork.Repository<IPlanFeatureRepository, PlanFeature>();
        private readonly IWalletRepository walletRepository = unitOfWork.Repository<IWalletRepository, Wallet>();
        public async Task ProcessPlansAsync()
        {
            await ResetDailyProposalUsageAsync();

            var subscriptions = await subscriptionRepository.GetExpireSubscription();
             var now = DateTime.UtcNow;
            foreach(var sub in subscriptions)
            {
                if (sub.PlanId == PlanSeeds.FreePlanId)
                {
                    //لو free صفر usage
                    sub.StartedAt = now;
                    sub.ExpiresAt = now.AddMonths(1);
                    await ResetUsageAsync(sub);
                }
                else
                {
                    var wallet = await walletRepository.GetByOwnerAsync(owner.User, sub.UserId);
                    var subMoney = sub.BillingPeriod==BillingPeriod.Monthly?sub.Plan.MonthlyPrice:sub.Plan.YearlyPrice;
                    if (sub.AutoRenew && wallet.Available >= subMoney)
                    {
                        // لو في فلوس في المحفظه صفر usage و اخصم من avalible
                        sub.StartedAt = DateTime.UtcNow;
                        sub.ExpiresAt = sub.BillingPeriod == BillingPeriod.Monthly
                            ? now.AddMonths(1)
                            : now.AddYears(1);
                        wallet.Available -= subMoney;
                        await ResetUsageAsync(sub);
                    }
                    else
                    {
                        // هانرجع لي free هانعدل sub و هانعدل usage
                        sub.PlanId = PlanSeeds.FreePlanId;
                        sub.BillingPeriod = BillingPeriod.Monthly;
                        sub.StartedAt = now;
                        sub.ExpiresAt = now.AddMonths(1);

                        await SyncUsageWithPlanAsync(
                            sub,
                            PlanSeeds.FreePlanId);
                    }
                }
            }
            await unitOfWork.SaveChangesAsync();
        }
        private async Task ResetUsageAsync(Domain.Entities.Plans.Subscription sub)
        {
            var usageRecords = await usageRecordRepository.GetUsageRecordsBySubId(sub.Id);
            foreach(var record in usageRecords)
            {
                record.Used = 0;
                record.PeriodStart = sub.StartedAt;
                record.PeriodEnd = sub.ExpiresAt!.Value;
            }
        }
        private async Task SyncUsageWithPlanAsync(
     Domain.Entities.Plans.Subscription subscription,
     Guid planId)
        {
            var usageRecords =
                await usageRecordRepository.GetUsageRecordsBySubId(subscription.Id);

            var planFeatures =
                await planFeatureRepository.GetByPlanIdAsync(planId);

            foreach (var usage in usageRecords)
            {
                var planFeature = planFeatures.FirstOrDefault(x =>
                    x.Feature == usage.Feature &&
                    x.IsEnabled);

                // الـ Feature مش موجودة في الـ Plan الجديد
                if (planFeature is null)
                {
                    usageRecordRepository.Delete(usage);
                    continue;
                }

                // موجودة → صفر الاستخدام
                usage.Used = 0;
                usage.PeriodStart = subscription.StartedAt;
                usage.PeriodEnd = subscription.ExpiresAt!.Value;
            }
        }
        private async Task ResetDailyProposalUsageAsync()
        {
            var usageRecords =
                await usageRecordRepository.GetActiveProposalUsagesAsync();


            foreach (var record in usageRecords)
            {
                record.Used = 0;

              
            }
        }
    }
}
