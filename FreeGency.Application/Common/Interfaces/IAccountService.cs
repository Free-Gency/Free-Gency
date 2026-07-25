using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Features.Account.Dtos;

namespace FreeGency.Application.Common.Interfaces;

public interface IAccountService
{
    Task<Result> ChangePasswordAsync(ChangePasswordRequestDto dto);
    Task<Result<ClientAccountResponseDto>> GetClientProfile();
    Task<Result<List<ProfileInterestDto>>> GetClientInterests();
    Task<Result> UpdateClientProfileAsync(UpdateClientAccountDto dto);
    Task<Result> CreateProfileClientAsync();
    Task<Result> CreateProfileDeveloperAsync();
    Task<Result> CompleteOnboardingAsync();
    Task<Result<string>> SwitchModeAsync();
    Task<Result<DeveloperAccountResponseDto>> GetDeveloperProfile();
    Task<ApiResponse> AddClientInterestsAsync(ProfileInterestsDto dto, CancellationToken ct = default);
    Task<ApiResponse> ReplaceClientInterestsAsync(ProfileInterestsDto dto, CancellationToken ct = default);
    Task<ApiResponse> AddDeveloperInterestsAsync(ProfileInterestsDto dto, CancellationToken ct = default);
    Task<ApiResponse> ReplaceDeveloperInterestsAsync(ProfileInterestsDto dto, CancellationToken ct = default);
    Task<ApiResponse> AddClientSpecialtiesAsync(ProfileSpecialtiesDto dto, CancellationToken ct = default);
    Task<ApiResponse> ReplaceClientSpecialtiesAsync(ProfileSpecialtiesDto dto, CancellationToken ct = default);
    Task<ApiResponse> AddDeveloperSpecialtiesAsync(ProfileSpecialtiesDto dto, CancellationToken ct = default);
    Task<ApiResponse> ReplaceDeveloperSpecialtiesAsync(ProfileSpecialtiesDto dto, CancellationToken ct = default);
    Task<ApiResponse> ReplaceClientSkillsAsync(ProfileSkillsDto dto, CancellationToken ct = default);
    Task<ApiResponse> ReplaceDeveloperSkillsAsync(ProfileSkillsDto dto, CancellationToken ct = default);
}
