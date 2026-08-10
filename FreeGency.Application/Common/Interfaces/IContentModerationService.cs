using FreeGency.Application.Features.Moderation.DTOs;
using FreeGency.Domain.Enums;

namespace FreeGency.Application.Common.Interfaces;

public interface IContentModerationService
{
    Task<(bool IsMuted, DateTime? Until)> GetMuteStatusAsync(Guid userId, CancellationToken ct = default);

    Task<ContentModerationResult> ModerateAndEnforceAsync(
        Guid userId,
        ModerationSourceType sourceType,
        Guid sourceId,
        string? content,
        string surface,
        Guid? clientProfileId,
        Guid? developerProfileId,
        CancellationToken ct = default);

    Task<ApiResponse<IReadOnlyList<ModerationCaseDto>>> ListOpenCasesAsync(CancellationToken ct = default);

    Task<ApiResponse> ResolveCaseAsync(Guid caseId, string? adminNote, Guid adminUserId, CancellationToken ct = default);

    Task<ApiResponse<MyModerationStatusDto>> GetMyStatusAsync(Guid userId, CancellationToken ct = default);
}
