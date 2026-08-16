namespace FreeGency.Application.Common.Interfaces;

public interface IMilestonePlanAiService
{
    Task<ApiResponse<Features.Milestones.DTOs.MilestonePlanAiAssistResponseDto>> AssistAsync(
        Guid projectId,
        Features.Milestones.DTOs.MilestonePlanAiAssistRequestDto request,
        CancellationToken ct = default);
}
