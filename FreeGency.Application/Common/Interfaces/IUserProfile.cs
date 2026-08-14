namespace FreeGency.Application.Common.Interfaces
{
    public interface IUserProfile
    {
        Task<ApiResponse<ClientProfileDto>> GetClientProfileAsync(Guid id, CancellationToken ct = default);
        Task<ApiResponse<DeveloperProfileDto>> GetDeveloperProfileAsync(Guid id, CancellationToken ct = default);
    }
}
