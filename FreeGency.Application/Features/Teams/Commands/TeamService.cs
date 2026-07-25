using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Common.Mappings.TeamsMapping;
using FreeGency.Application.Features.Teams.Dtos;
using FreeGency.Infrastructure.Integrations.Cloudinary;
using FreeGency.Infrastructure.Interfaces;

namespace FreeGency.Application.Features.Teams.Commands
{
    public partial class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStorageService _storageService;
        private readonly ICurrentUserService _currentUserService;

        public TeamService(IUnitOfWork unitOfWork, IStorageService storageService, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _storageService = storageService;
            _currentUserService = currentUserService;
            _teamRepository = _unitOfWork.Repository<ITeamRepository, Team>();
        }

        public async Task<ApiResponse<Guid>> CreateAsync(CreateTeamDto dto, CancellationToken ct = default)
        {
            string teamCode;
            do
            {
                teamCode = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            } while (await _teamRepository.TeamCodeExistsAsync(teamCode, ct));

            string? logoUrl = null;
            if (dto.Logo is not null)
            {
                try
                {
                    logoUrl = (await _storageService.UploadAsync(dto.Logo, StorageFolders.Category, ct)).Url;
                }
                catch (Exception)
                {
                    return ApiResponse.Failure<Guid>(AppError.FileUploadFailed(dto.Logo.FileName));
                }
            }

            var team = dto.ToEntity(_currentUserService.UserId, teamCode, logoUrl);

            var categories = dto.Categories.Select(c => (c.CategoryId, c.IsPrimary));
            await _teamRepository.AddWithTaxonomyAsync(team, categories, dto.SkillIds, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse.Success(team.Id, "Team created successfully.");
        }

        public async Task<ApiResponse> UpdateAsync(UpdateTeamDto dto, CancellationToken ct = default)
        {
            var team = await _teamRepository.GetByIdAsync(dto.Id, ct);
            if (team is null)
                return ApiResponse.Failure(AppError.NotFound(nameof(Team), dto.Id));

            if (team.OwnerUserId != _currentUserService.UserId)
                return ApiResponse.Failure(AppError.Forbidden("You are not authorized to perform this action on this team."));

            team.Name = dto.Name;
            team.AboutUs = dto.AboutUs;

            if (dto.Logo is not null)
            {
                try
                {
                    team.Logo = (await _storageService.UploadAsync(dto.Logo, StorageFolders.Category, ct)).Url;
                }
                catch (Exception)
                {
                    return ApiResponse.Failure(AppError.FileUploadFailed(dto.Logo.FileName));
                }
            }

            _teamRepository.Update(team);
            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse.Success("Team updated successfully.");
        }

        public async Task<ApiResponse> ReplaceCategoriesAsync(Guid teamId, UpdateTeamCategoriesDto dto, CancellationToken ct = default)
        {
            var team = await _teamRepository.GetByIdAsync(teamId, ct);
            if (team is null)
                return ApiResponse.Failure(AppError.NotFound(nameof(Team), teamId));

            if (team.OwnerUserId != _currentUserService.UserId)
                return ApiResponse.Failure(AppError.Forbidden("You are not authorized to perform this action on this team."));

            var categories = dto.Categories.Select(c => (c.CategoryId, c.IsPrimary));
            await _teamRepository.ReplaceCategoriesAsync(teamId, categories, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse.Success("Team categories updated successfully.");
        }

        public async Task<ApiResponse> ReplaceSpecialtiesAsync(Guid teamId, UpdateTeamSpecialtiesDto dto, CancellationToken ct = default)
        {
            var team = await _teamRepository.GetByIdAsync(teamId, ct);
            if (team is null)
                return ApiResponse.Failure(AppError.NotFound(nameof(Team), teamId));

            if (team.OwnerUserId != _currentUserService.UserId)
                return ApiResponse.Failure(AppError.Forbidden("You are not authorized to perform this action on this team."));

            await _teamRepository.ReplaceSpecialtiesAsync(teamId, dto.SpecialtyIds, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse.Success("Team specialties updated successfully.");
        }

        public async Task<ApiResponse> ReplaceSkillsAsync(Guid teamId, UpdateTeamSkillsDto dto, CancellationToken ct = default)
        {
            var team = await _teamRepository.GetByIdAsync(teamId, ct);
            if (team is null)
                return ApiResponse.Failure(AppError.NotFound(nameof(Team), teamId));

            if (team.OwnerUserId != _currentUserService.UserId)
                return ApiResponse.Failure(AppError.Forbidden("You are not authorized to perform this action on this team."));

            await _teamRepository.ReplaceSkillsAsync(teamId, dto.SkillIds, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse.Success("Team skills updated successfully.");
        }
    }
}