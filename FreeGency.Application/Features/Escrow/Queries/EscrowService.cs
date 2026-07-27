using FreeGency.Application.Features.Escrow.DTOs;

namespace FreeGency.Application.Features.Escrow.Commands;

public partial class EscrowService
{
    public async Task<ApiResponse<EscrowDto>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
    {
        var project = await _projectRepo.GetByIdAsync(projectId, ct);
        if (project is null)
            return ApiResponse.Failure<EscrowDto>(AppError.NotFound(nameof(Project), projectId));

        var escrow = await _escrowRepo.GetByProjectIdAsync(projectId, ct);
        if (escrow is null)
            return ApiResponse.Failure<EscrowDto>(AppError.NotFound("EscrowHold", projectId));

        return ApiResponse.Success(_mapper.Map<EscrowDto>(escrow));
    }
}