using FreeGency.Application.Common.Interfaces;

namespace FreeGency.Api.BackgroundJobs;

/// <summary>
/// Auto-releases submitted milestones after the review window (default 14 days).
/// </summary>
public sealed class MilestoneAutoReleaseWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<MilestoneAutoReleaseWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var milestones = scope.ServiceProvider.GetRequiredService<IMilestoneService>();
                var count = await milestones.AutoReleaseDueMilestonesAsync(stoppingToken);
                if (count > 0)
                    logger.LogInformation("Auto-released {Count} milestone(s)", count);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Milestone auto-release worker failed");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}
