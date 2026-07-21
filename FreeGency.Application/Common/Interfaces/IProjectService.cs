namespace FreeGency.Application.Common.Interfaces
{
    public interface IProjectService
    {
        // Reads
        Task<Result<ProjectDto>> BrowseAsync(FilterProjectsRequestDto filterRequest, CancellationToken ct = default);
        Task<Result<ProjectDto>> GetDetailsAsync(Guid id, CancellationToken ct = default);



        // Writes
        Task<Result> CreateAsync(CreateProjectDto newProject, CancellationToken ct = default);
        Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
        Task<Result> EditAsync(ProjectDto updatedProject, CancellationToken ct = default);



    }
}
