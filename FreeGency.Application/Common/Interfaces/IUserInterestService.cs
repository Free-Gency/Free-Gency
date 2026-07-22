using FreeGency.Application.Features.categories.Dtos;
using FreeGency.Application.Features.userInterests.Dtos;

namespace FreeGency.Application.Common.Interfaces;

public interface IUserInterestService
{
    Task<ApiResponse<IEnumerable<CategoryDto>>> GetMyInterestsAsync(CancellationToken ct = default);
    Task<ApiResponse> ReplaceMyInterestsAsync(ReplaceUserInterestsDto dto, CancellationToken ct = default);
}
