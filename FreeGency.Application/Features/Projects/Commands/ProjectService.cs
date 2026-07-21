using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Common.Results;
using FreeGency.Application.Features.Projects.DTOs;

namespace FreeGency.Application.Features.Projects.Commands
{
    public partial class ProjectService : IProjectService
    {
        public Task<Result> CreateProjectAsync(CreateProjectDto newProject, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<Result<ProjectDto>> BrowseProjects()
            => throw new NotImplementedException();

        public Task<Result<ProjectDto>> GetProjectDetailsAsync(Guid id, CancellationToken ct = default)
            => throw new NotImplementedException();
    }
}
