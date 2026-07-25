using FreeGency.Application.Features.ProjectFiles.DTOs;
using FreeGency.Infrastructure.Integrations.Cloudinary;

namespace FreeGency.Application.Features.ProjectFiles.Commands;

public sealed class ProjectFileService : IProjectFileService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageService _storageService;
    private readonly ICurrentUserService _currentUser;
    private readonly IProjectRepository _projectRepo;
    private readonly IProjectFileRepository _projectFileRepo;

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
    }

    public async Task<ApiResponse<IEnumerable<ProjectFileDto>>> ListByProjectAsync(
        Guid projectId,
        FileKind? kind = null,
        CancellationToken ct = default)
    {
        var project = await _projectRepo.GetByIdAsync(projectId, ct);
        if (project is null)
            return ApiResponse.Failure<IEnumerable<ProjectFileDto>>(AppError.NotFound(nameof(Project), projectId));

        if (project.ClientId != _currentUser.UserId)
            return ApiResponse.Failure<IEnumerable<ProjectFileDto>>(AppError.Forbidden("You do not own this project."));

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

        if (project.ClientId != _currentUser.UserId)
            return ApiResponse.Failure<IEnumerable<ProjectFileDto>>(AppError.Forbidden("You do not own this project."));

        if (request.Files is null || request.Files.Length == 0)
            return ApiResponse.Failure<IEnumerable<ProjectFileDto>>(
                AppError.Validation("Please select at least one file."));

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

        if (project.ClientId != _currentUser.UserId)
            return ApiResponse.Failure(AppError.Forbidden("You do not own this project."));

        _projectFileRepo.Delete(file);
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success("File removed successfully.");
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
