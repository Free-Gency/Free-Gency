namespace FreeGency.Application.Features.Projects.Commands
{
    // Queries
    public partial class ProjectService
    {
        public Task<ApiResponse<ProjectDto>> BrowseAsync(FilterProjectsRequestDto filterRequest, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<ProjectDto>> GetDetailsAsync(Guid id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<IEnumerable<SavedProjectsDto>>> GetMyProjectsAsync(string role, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<IEnumerable<SavedProjectsDto>>> GetSavedProjectsAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
