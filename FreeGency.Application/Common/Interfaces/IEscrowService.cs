
using FreeGency.Application.Features.Escrow.DTOs;

namespace FreeGency.Application.Common.Interfaces;

public interface IEscrowService
{
    Task<ApiResponse<EscrowDto>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
}