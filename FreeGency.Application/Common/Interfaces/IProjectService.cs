namespace FreeGency.Application.Common.Interfaces
{
    public interface IProjectService
    {
        // Reads
        Task<ApiResponse<PaginatedResult<ProjectDto>>> BrowseAsync(FilterProjectsRequestDto filterRequest, CancellationToken ct = default);
        Task<ApiResponse<ProjectDto>> GetDetailsAsync(Guid id, CancellationToken ct = default);
        Task<ApiResponse<IEnumerable<ProjectDto>>> GetSavedProjectsAsync(CancellationToken ct = default);
        Task<ApiResponse<PaginatedResult<ProjectDto>>> GetMyProjectsAsync(MyProjectsRequestDto request, CancellationToken ct = default);
        Task<ApiResponse<MyProjectsSummaryDto>> GetMyProjectsSummaryAsync(string role, Guid? teamId = null, CancellationToken ct = default);


        // Writes
        Task<ApiResponse<Guid>> CreateAsync(CreateProjectRequestDto newProject, CancellationToken ct = default);
        Task<ApiResponse<Guid>> CreateForClientAsync(CreateProjectRequestDto newProject, Guid clientUserId, CancellationToken ct = default);
        Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default);
        Task<ApiResponse> EditAsync(UpdateProjectRequestDto updatedProject, CancellationToken ct = default);
        Task<ApiResponse> PublishAsync(Guid id, CancellationToken ct = default);
        Task<ApiResponse> PublishForClientAsync(Guid id, Guid clientUserId, CancellationToken ct = default);
        Task<ApiResponse> SaveAsync(Guid id, CancellationToken ct = default);
        Task<ApiResponse> UnSaveAsync(Guid id, CancellationToken ct = default);
        Task<ApiResponse> ReplaceSkillsAsync(Guid id, IEnumerable<Guid> skillIds, CancellationToken ct = default);
    }
}
