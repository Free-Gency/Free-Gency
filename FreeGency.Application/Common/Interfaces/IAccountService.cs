using FreeGency.Application.Features.Account.Dtos;

namespace FreeGency.Application.Common.Interfaces;

public interface IAccountService
{
    Task<Result<ClientAccountResponseDto>> GetClientProfile();
    Task<Result> UpdateClientProfileAsync(UpdateClientAccountDto dto);
    Task<ApiResponse> AddClientInterestsAsync(ProfileInterestsDto dto, CancellationToken ct = default);
    Task<ApiResponse> ReplaceClientInterestsAsync(ProfileInterestsDto dto, CancellationToken ct = default);
    Task<ApiResponse> AddDeveloperInterestsAsync(ProfileInterestsDto dto, CancellationToken ct = default);
    Task<ApiResponse> ReplaceDeveloperInterestsAsync(ProfileInterestsDto dto, CancellationToken ct = default);
}
