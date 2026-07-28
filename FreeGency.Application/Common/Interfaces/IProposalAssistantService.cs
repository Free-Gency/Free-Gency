using FreeGency.Application.Features.ProposalAssistant.Dtos;

namespace FreeGency.Application.Common.Interfaces;

public interface IProposalAssistantService
{
    Task<ApiResponse<ProposalAssistantResponseDto>> AskAsync(
        Guid projectId,
        ProposalAssistantRequestDto request,
        CancellationToken ct = default);
}
