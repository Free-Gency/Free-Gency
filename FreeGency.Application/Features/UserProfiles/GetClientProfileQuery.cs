namespace FreeGency.Application.Features.UserProfiles
{
    public partial class UserProfile
    {
        public async Task<ApiResponse<ClientProfileDto>> GetClientProfileAsync(Guid id, CancellationToken ct = default)
        {
            var clientProfile = await _clientProfileRepo
               .GetByUserIdWithSkillsAndInterestsAsync(id, ct);

            if (clientProfile == null)
                return ApiResponse.Failure<ClientProfileDto>(
                    AppError.NotFound(nameof(ClientProfile), id));

            return ApiResponse.Success(
                _mapper.Map<ClientProfileDto>(clientProfile));
        }
    }
}