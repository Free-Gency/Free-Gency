using FreeGency.Application.Features.Suggestions.DTOs;

namespace FreeGency.Application.Common.Interfaces;

public interface ISuggestionService
{
    Task<ApiResponse<TeamsForMeResponseDto>> SuggestTeamsForMeAsync(int topK = 10, CancellationToken ct = default);

    Task<ApiResponse<ProjectCandidatesResponseDto>> SuggestCandidatesForProjectAsync(
        Guid projectId,
        int topK = 10,
        CancellationToken ct = default);

    /// <summary>System/Hangfire path — validates ownership via clientUserId instead of HTTP current user.</summary>
    Task<ApiResponse<ProjectCandidatesResponseDto>> SuggestCandidatesForProjectAsSystemAsync(
        Guid projectId,
        Guid clientUserId,
        int topK = 10,
        CancellationToken ct = default);

    Task<ApiResponse<ReindexResultDto>> ReindexAllAsync(CancellationToken ct = default);

    Task IndexDeveloperAsync(Guid userId, CancellationToken ct = default);
    Task IndexTeamAsync(Guid teamId, CancellationToken ct = default);
    Task IndexTeamJobAsync(Guid jobId, CancellationToken ct = default);
    Task RemoveTeamJobAsync(Guid jobId, Guid teamId, CancellationToken ct = default);
    Task IndexProjectAsync(Guid projectId, CancellationToken ct = default);
    Task RemoveProjectAsync(Guid projectId, CancellationToken ct = default);
}
