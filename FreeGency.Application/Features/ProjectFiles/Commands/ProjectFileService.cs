using FreeGency.Application.Features.ProjectFiles.DTOs;
using FreeGency.Domain.Interfaces.Repositories.Teams;
using FreeGency.Infrastructure.Integrations.Cloudinary;

namespace FreeGency.Application.Features.ProjectFiles.Commands;

public sealed class ProjectFileService : IProjectFileService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageService _storageService;
    private readonly ICurrentUserService _currentUser;
    private readonly IProjectRepository _projectRepo;
    private readonly IProjectFileRepository _projectFileRepo;
    private readonly IMilestoneRepository _milestoneRepo;
    private readonly ITeamMemberRepository _teamMemberRepo;
    private readonly IUserRepository _userRepo;

    public ProjectFileService(
        IUnitOfWork unitOfWork,
        IStorageService storageService,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _storageService = storageService;
        _currentUser = currentUser;
        _projectRepo = _unitOfWork.Repository<IProjectRepository, Project>();
        _projectFileRepo = _unitOfWork.Repository<IProjectFileRepository, ProjectFile>();
        _milestoneRepo = _unitOfWork.Repository<IMilestoneRepository, Milestone>();
        _teamMemberRepo = _unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
        _userRepo = _unitOfWork.Repository<IUserRepository, User>();
    }

    public async Task<ApiResponse<IEnumerable<ProjectFileDto>>> ListByProjectAsync(
        Guid projectId,
        FileKind? kind = null,
        CancellationToken ct = default)
    {
        var project = await _projectRepo.GetByIdAsync(projectId, ct);
        if (project is null)
            return ApiResponse.Failure<IEnumerable<ProjectFileDto>>(AppError.NotFound(nameof(Project), projectId));

        if (!await CanAccessProjectFilesAsync(project, ct))
            return ApiResponse.Failure<IEnumerable<ProjectFileDto>>(
                AppError.Forbidden("You do not have access to this project's files."));

        var files = await _projectFileRepo.GetByProjectIdAsync(projectId, kind, ct);
        return ApiResponse.Success(files.Select(ToDto));
    }

    public async Task<ApiResponse<IEnumerable<ProjectFileDto>>> UploadAsync(
        Guid projectId,
        UploadProjectFilesRequestDto request,
        CancellationToken ct = default)
    {
        var project = await _projectRepo.GetByIdAsync(projectId, ct);
        if (project is null)
            return ApiResponse.Failure<IEnumerable<ProjectFileDto>>(AppError.NotFound(nameof(Project), projectId));

        var isClient = project.ClientId == _currentUser.UserId;
        var isWorker = await IsProjectWorkerAsync(project, ct);

        if (!isClient && !isWorker)
            return ApiResponse.Failure<IEnumerable<ProjectFileDto>>(
                AppError.Forbidden("Only the client or hired team can upload project files."));

        if (isClient)
        {
            var profileError = await RequireActiveProfileModeAsync(profileMode.Client, ct);
            if (profileError is not null)
                return ApiResponse.Failure<IEnumerable<ProjectFileDto>>(profileError);
        }
        else
        {
            var profileError = await RequireActiveProfileModeAsync(profileMode.Developer, ct);
            if (profileError is not null)
                return ApiResponse.Failure<IEnumerable<ProjectFileDto>>(profileError);

            // Hired developers upload deliverables (and shared docs), not the client brief.
            if (request.FileKind is FileKind.Brief)
                return ApiResponse.Failure<IEnumerable<ProjectFileDto>>(
                    AppError.Forbidden("Developers can upload Deliverable, Shared, or Other files — not Brief."));
        }

        if (request.Files is null || request.Files.Length == 0)
            return ApiResponse.Failure<IEnumerable<ProjectFileDto>>(
                AppError.Validation("Please select at least one file."));

        if (request.MilestoneId.HasValue)
        {
            var milestone = await _milestoneRepo.GetByIdAsync(request.MilestoneId.Value, ct);
            if (milestone is null || milestone.ProjectId != projectId)
            {
                return ApiResponse.Failure<IEnumerable<ProjectFileDto>>(
                    AppError.Validation("Milestone does not belong to this project."));
            }
        }

        var created = new List<ProjectFile>();

        foreach (var file in request.Files)
        {
            UploadedAsset uploaded;
            try
            {
                uploaded = await _storageService.UploadAsync(file, StorageFolders.ProjectFiles, ct);
            }
            catch (Exception)
            {
                return ApiResponse.Failure<IEnumerable<ProjectFileDto>>(
                    AppError.FileUploadFailed(file.FileName));
            }

            var entity = new ProjectFile
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                MilestoneId = request.MilestoneId,
                UploadedByUserId = _currentUser.UserId,
                FileName = string.IsNullOrWhiteSpace(uploaded.FileName) ? file.FileName : uploaded.FileName,
                FileUrl = uploaded.Url,
                FileKind = request.FileKind,
            };

            await _projectFileRepo.AddAsync(entity, ct);
            created.Add(entity);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success(
            created.Select(ToDto),
            "Files uploaded successfully.");
    }

    public async Task<ApiResponse> DeleteAsync(Guid fileId, CancellationToken ct = default)
    {
        var file = await _projectFileRepo.GetByIdAsync(fileId, ct);
        if (file is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(ProjectFile), fileId));

        var project = await _projectRepo.GetByIdAsync(file.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Project), file.ProjectId));

        var isClient = project.ClientId == _currentUser.UserId;
        var isUploader = file.UploadedByUserId == _currentUser.UserId;
        var isWorker = await IsProjectWorkerAsync(project, ct);

        if (!isClient && !(isWorker && isUploader))
            return ApiResponse.Failure(AppError.Forbidden("You cannot delete this file."));

        if (isClient)
        {
            var profileError = await RequireActiveProfileModeAsync(profileMode.Client, ct);
            if (profileError is not null)
                return ApiResponse.Failure(profileError);
        }
        else
        {
            var profileError = await RequireActiveProfileModeAsync(profileMode.Developer, ct);
            if (profileError is not null)
                return ApiResponse.Failure(profileError);
        }

        _projectFileRepo.Delete(file);
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success("File removed successfully.");
    }

    private async Task<bool> CanAccessProjectFilesAsync(Project project, CancellationToken ct)
    {
        if (project.ClientId == _currentUser.UserId)
            return true;

        return await IsProjectWorkerAsync(project, ct);
    }

    private async Task<bool> IsProjectWorkerAsync(Project project, CancellationToken ct)
    {
        if (project.AssignedUserId == _currentUser.UserId)
            return true;

        if (project.AssignedTeamId is not null)
            return await _teamMemberRepo.IsMemberAsync(project.AssignedTeamId.Value, _currentUser.UserId, ct);

        return false;
    }

    private async Task<AppError?> RequireActiveProfileModeAsync(profileMode required, CancellationToken ct)
    {
        var active = await _userRepo.GetActiveProfileAsync(_currentUser.UserId, ct);
        if (active is null)
        {
            return AppError.Validation(
                "An active profile is required. Create or switch to a Client or Developer profile.");
        }

        if (active.Value.Mode != required)
        {
            return AppError.Forbidden(required == profileMode.Client
                ? "Switch to Client profile to perform this action."
                : "Switch to Developer profile to perform this action.");
        }

        return null;
    }

    private static ProjectFileDto ToDto(ProjectFile file) => new()
    {
        Id = file.Id,
        ProjectId = file.ProjectId,
        MilestoneId = file.MilestoneId,
        UploadedByUserId = file.UploadedByUserId,
        FileName = file.FileName,
        FileUrl = file.FileUrl,
        FileKind = file.FileKind,
        CreatedAt = file.CreatedAt,
    };
}
