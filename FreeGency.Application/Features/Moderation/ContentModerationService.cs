using FreeGency.AI.Moderation;
using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Features.Moderation.DTOs;
using FreeGency.Application.Features.NotificationFeature.Dtos;
using FreeGency.Domain.Enums;
using FreeGency.Infrastructure.Interfaces;
using FreeGency.Infrastructure.Persistence.Context;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Application.Features.Moderation;

public sealed class ContentModerationService : IContentModerationService
{
    private const int StrikeWindowDays = 30;
    private const int StrikeThreshold = 3;
    private static readonly TimeSpan MuteDuration = TimeSpan.FromHours(24);

    private readonly IModerationAgent _agent;
    private readonly ApplicationDbContext _db;
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notifications;

    public ContentModerationService(
        IModerationAgent agent,
        ApplicationDbContext db,
        IUnitOfWork uow,
        INotificationService notifications)
    {
        _agent = agent;
        _db = db;
        _uow = uow;
        _notifications = notifications;
    }

    public async Task<(bool IsMuted, DateTime? Until)> GetMuteStatusAsync(Guid userId, CancellationToken ct = default)
    {
        var until = await _db.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.ModerationMutedUntil)
            .FirstOrDefaultAsync(ct);

        if (until is null || until <= DateTime.UtcNow)
            return (false, null);

        return (true, until);
    }

    public async Task<ContentModerationResult> ModerateAndEnforceAsync(
        Guid userId,
        ModerationSourceType sourceType,
        Guid sourceId,
        string? content,
        string surface,
        Guid? clientProfileId,
        Guid? developerProfileId,
        CancellationToken ct = default)
    {
        var (muted, until) = await GetMuteStatusAsync(userId, ct);
        if (muted)
        {
            return new ContentModerationResult
            {
                Action = ModerationAction.BlockSubmit,
                Status = ModerationStatus.Hidden,
                WarningMessage = $"You are temporarily restricted from posting until {until:u} due to repeated policy violations.",
                IsMuted = true,
                MutedUntil = until
            };
        }

        var text = content ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            return new ContentModerationResult
            {
                Action = ModerationAction.Allow,
                Status = ModerationStatus.Visible,
                SafeText = text
            };
        }

        var decision = await _agent.ModerateAsync(new ModerationRequest
        {
            Content = text,
            Surface = surface
        }, ct);

        var status = decision.Action switch
        {
            ModerationAction.Allow => ModerationStatus.Visible,
            ModerationAction.Redact => ModerationStatus.Redacted,
            ModerationAction.Hide => ModerationStatus.Hidden,
            ModerationAction.BlockSubmit => ModerationStatus.Hidden,
            _ => ModerationStatus.Visible
        };

        var needsCase = decision.Action != ModerationAction.Allow
                        || decision.Categories.Any(c => c != ModerationCategory.Clean);

        Guid? caseId = null;
        if (needsCase)
        {
            var needsAdmin = decision.Action == ModerationAction.BlockSubmit
                             || decision.Categories.Contains(ModerationCategory.OffPlatform)
                             || decision.Confidence < 0.55f;

            var moderationCase = new ModerationCase
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SourceType = sourceType,
                SourceId = sourceId,
                ContentSnapshot = text.Length > 4000 ? text[..4000] : text,
                Categories = string.Join(',', decision.Categories.Select(c => c.ToString())),
                Confidence = decision.Confidence,
                Action = decision.Action,
                Status = needsAdmin ? ModerationCaseStatus.NeedsAdmin : ModerationCaseStatus.AutoResolved,
                UserMessage = decision.UserMessage,
                AdminSummary = decision.AdminSummary,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString()
            };

            _db.ModerationCases.Add(moderationCase);
            caseId = moderationCase.Id;

            if (decision.Action is ModerationAction.Hide or ModerationAction.BlockSubmit or ModerationAction.Redact)
            {
                var primary = decision.Categories.FirstOrDefault(c => c != ModerationCategory.Clean);
                if (primary == ModerationCategory.Clean)
                    primary = ModerationCategory.Spam;

                _db.UserModerationStrikes.Add(new UserModerationStrike
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ModerationCaseId = moderationCase.Id,
                    PrimaryCategory = primary,
                    Reason = decision.AdminSummary,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId.ToString()
                });

                var since = DateTime.UtcNow.AddDays(-StrikeWindowDays);
                var priorStrikes = await _db.UserModerationStrikes
                    .CountAsync(s => s.UserId == userId && s.CreatedAt >= since, ct);
                var strikeCount = priorStrikes + 1;

                if (strikeCount >= StrikeThreshold)
                {
                    var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
                    if (user is not null)
                    {
                        user.ModerationMutedUntil = DateTime.UtcNow.Add(MuteDuration);
                        moderationCase.Status = ModerationCaseStatus.NeedsAdmin;
                        until = user.ModerationMutedUntil;
                        muted = true;
                    }
                }

                var warning = string.IsNullOrWhiteSpace(decision.UserMessage)
                    ? "Your content violated FreeGency community rules."
                    : decision.UserMessage;

                if (muted && until is not null)
                {
                    warning =
                        $"{warning} You are temporarily restricted from posting until {until:u} (applies to your whole FreeGency account).";
                }

                var title = primary switch
                {
                    ModerationCategory.PiiContact => "Contact details not allowed",
                    ModerationCategory.OffPlatform => "Off-platform activity blocked",
                    ModerationCategory.Abuse or ModerationCategory.Harassment => "Inappropriate language blocked",
                    ModerationCategory.Spam => "Spam content blocked",
                    _ => "Community guidelines warning"
                };

                BackgroundJob.Enqueue(() => _notifications.CreateNotification(new CreateNotificationRequest
                {
                    Title = title,
                    Body = warning,
                    Type = NotificationType.System,
                    UserId = userId,
                    ClientProfileId = clientProfileId,
                    DeveloperProfileId = developerProfileId,
                    ActionUrl = "/settings/community-guidelines"
                }));
            }

            await _uow.SaveChangesAsync(ct);
        }

        var safeText = status switch
        {
            ModerationStatus.Visible => text,
            ModerationStatus.Redacted => decision.RedactedText ?? ModerationHeuristics.Redact(text),
            _ => null
        };

        return new ContentModerationResult
        {
            Action = decision.Action,
            Status = status,
            SafeText = safeText,
            WarningMessage = decision.Action == ModerationAction.Allow ? null : decision.UserMessage,
            CaseId = caseId,
            IsMuted = muted,
            MutedUntil = until
        };
    }

    public async Task<ApiResponse<IReadOnlyList<ModerationCaseDto>>> ListOpenCasesAsync(CancellationToken ct = default)
    {
        var items = await _db.ModerationCases
            .AsNoTracking()
            .Where(c => c.Status == ModerationCaseStatus.NeedsAdmin)
            .OrderByDescending(c => c.CreatedAt)
            .Take(100)
            .Select(c => new ModerationCaseDto
            {
                Id = c.Id,
                UserId = c.UserId,
                SourceType = c.SourceType.ToString(),
                SourceId = c.SourceId,
                Categories = c.Categories,
                Confidence = c.Confidence,
                Action = c.Action.ToString(),
                Status = c.Status.ToString(),
                UserMessage = c.UserMessage,
                AdminSummary = c.AdminSummary,
                AdminNote = c.AdminNote,
                CreatedAt = c.CreatedAt,
                ResolvedAt = c.ResolvedAt
            })
            .ToListAsync(ct);

        return ApiResponse.Success<IReadOnlyList<ModerationCaseDto>>(items);
    }

    public async Task<ApiResponse> ResolveCaseAsync(
        Guid caseId,
        string? adminNote,
        Guid adminUserId,
        CancellationToken ct = default)
    {
        var entity = await _db.ModerationCases.FirstOrDefaultAsync(c => c.Id == caseId, ct);
        if (entity is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(ModerationCase), caseId));

        entity.Status = ModerationCaseStatus.Resolved;
        entity.ResolvedAt = DateTime.UtcNow;
        entity.ResolvedByUserId = adminUserId;
        entity.AdminNote = adminNote;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = adminUserId.ToString();

        await _uow.SaveChangesAsync(ct);
        return ApiResponse.Success("Moderation case resolved.");
    }

    public async Task<ApiResponse<MyModerationStatusDto>> GetMyStatusAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var (isMuted, until) = await GetMuteStatusAsync(userId, ct);
        var since = DateTime.UtcNow.AddDays(-StrikeWindowDays);

        var strikes = await _db.UserModerationStrikes
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.CreatedAt >= since)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new
            {
                s.Id,
                s.PrimaryCategory,
                s.Reason,
                s.CreatedAt,
                UserMessage = s.ModerationCase != null ? s.ModerationCase.UserMessage : null,
                SourceType = s.ModerationCase != null ? s.ModerationCase.SourceType.ToString() : string.Empty
            })
            .Take(20)
            .ToListAsync(ct);

        var strikeCount = strikes.Count;
        var remaining = Math.Max(0, StrikeThreshold - strikeCount);
        var hasViolations = strikeCount > 0 || isMuted;

        var dto = new MyModerationStatusDto
        {
            HasViolations = hasViolations,
            StrikeCount = strikeCount,
            StrikeThreshold = StrikeThreshold,
            StrikeWindowDays = StrikeWindowDays,
            StrikesRemainingUntilRestriction = remaining,
            IsRestricted = isMuted,
            RestrictedUntil = until,
            RestrictionSummary = isMuted && until is not null
                ? $"Posting is temporarily restricted until {until:u}. This applies to your whole FreeGency account (Client and Developer)."
                : strikeCount > 0
                    ? $"{remaining} more warning{(remaining == 1 ? "" : "s")} in the next {StrikeWindowDays} days before a temporary posting restriction."
                    : "No active warnings.",
            RecentStrikes = strikes.Select(s => new MyModerationStrikeDto
            {
                Id = s.Id,
                Category = s.PrimaryCategory.ToString(),
                CategoryLabel = CategoryLabel(s.PrimaryCategory),
                Reason = string.IsNullOrWhiteSpace(s.Reason) ? CategoryLabel(s.PrimaryCategory) : s.Reason,
                UserMessage = s.UserMessage,
                SourceType = s.SourceType,
                CreatedAt = s.CreatedAt
            }).ToList()
        };

        return ApiResponse.Success(dto);
    }

    private static string CategoryLabel(ModerationCategory category) => category switch
    {
        ModerationCategory.PiiContact => "Contact details shared",
        ModerationCategory.OffPlatform => "Off-platform activity",
        ModerationCategory.Abuse => "Inappropriate language",
        ModerationCategory.Harassment => "Harassment",
        ModerationCategory.Spam => "Spam",
        _ => "Policy warning"
    };
}
