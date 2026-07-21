namespace FreeGency.Application.Common.Interfaces
{
    public interface IProjectService
    {
        Task<Result> CreateProjectAsync(CreateProjectDto newProject, CancellationToken ct = default);
        Task<Result<ProjectDto>> BrowseProjects();
        Task<Result<ProjectDto>> GetProjectDetailsAsync(Guid id, CancellationToken ct = default);
    }
}
