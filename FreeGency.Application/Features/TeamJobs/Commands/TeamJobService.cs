using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Common.Mappings.TeamJobsMapping;
using FreeGency.Application.Features.TeamJobs.Dtos;
using FreeGency.Domain.Interfaces.Repositories.Teams;
using FreeGency.Infrastructure.Interfaces;

namespace FreeGency.Application.Features.TeamJobs.Commands;

// Commands
public partial class TeamJobService : ITeamJobService
{
    private readonly ITeamJobRepository _teamJobRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly ISkillRepository _skillRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public TeamJobService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _teamJobRepository = _unitOfWork.Repository<ITeamJobRepository, TeamJob>();
        _teamRepository = _unitOfWork.Repository<ITeamRepository, Team>();
        _skillRepository = _unitOfWork.Repository<ISkillRepository, Skill>();
    }

    public async Task<ApiResponse<Guid>> CreateAsync(Guid teamId, CreateTeamJobDto dto, CancellationToken ct = default)
    {
        if (!await _teamRepository.ExistsAsync(teamId, ct))
            return ApiResponse.Failure<Guid>(AppError.NotFound(nameof(Team), teamId));

        var job = dto.ToEntity(teamId, _currentUserService.UserId);

        await _teamJobRepository.AddWithSkillsAsync(job, dto.SkillIds, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success(job.Id, "Team job created successfully.");
    }

    public async Task<ApiResponse> UpdateAsync(UpdateTeamJobDto dto, CancellationToken ct = default)
    {
        var job = await _teamJobRepository.GetByIdAsync(dto.Id, ct);

        if (job is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(TeamJob), dto.Id));

        job.Title = dto.Title;
        job.Description = dto.Description;

        _teamJobRepository.Update(job);
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success("Team job updated successfully.");
    }

    public async Task<ApiResponse> UpdateSkillsAsync(UpdateTeamJobSkillsDto dto, CancellationToken ct = default)
    {
        var job = await _teamJobRepository.GetByIdWithDetailsAsync(dto.Id, ct);

        if (job is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(TeamJob), dto.Id));

        _teamJobRepository.Delete(job);

        var newJob = new TeamJob
        {
            Id = job.Id,
            TeamId = job.TeamId,
            Title = job.Title,
            Description = job.Description,
            Status = job.Status,
            CreatedByUserId = job.CreatedByUserId,
            CreatedAt = job.CreatedAt,
            CreatedBy = job.CreatedBy
        };

        await _teamJobRepository.AddWithSkillsAsync(newJob, dto.SkillIds, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success("Team job skills updated successfully.");
    }

    public async Task<ApiResponse> CloseAsync(Guid id, CancellationToken ct = default)
    {
        var job = await _teamJobRepository.GetByIdAsync(id, ct);

        if (job is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(TeamJob), id));

        if (job.Status != TeamJobStatus.open)
            return ApiResponse.Failure(AppError.TeamJobAlreadyClosed(id));

        await _teamJobRepository.CloseAsync(id, DateTime.UtcNow, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success("Team job closed successfully.");
    }
}
