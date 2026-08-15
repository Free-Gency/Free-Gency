namespace FreeGency.Application.Common.Interfaces
{
    public interface IUserProfile
    {
        Task<ApiResponse<ClientProfileDto>> GetClientProfileAsync(Guid id, CancellationToken ct = default);
    }
}
