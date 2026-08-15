using FreeGency.Application.Features.HirePy.Dtos;

namespace FreeGency.Application.Common.Interfaces
{
    public interface IHirePySessionService
    {
        Task<ApiResponse<StartHirePySessionResponseDto>> StartAsync(
            StartHirePySessionRequestDto request,
            CancellationToken ct = default);

        Task<ApiResponse<HirePySessionDto>> GetAsync(Guid sessionId, CancellationToken ct = default);

        Task<ApiResponse<IReadOnlyList<HirePySessionDto>>> GetMineAsync(CancellationToken ct = default);

        Task ProcessProjectGenerationAsync(Guid sessionId, CancellationToken ct = default);
    }
}
