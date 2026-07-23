using FreeGency.Application.Common.Errors;
using FreeGency.Application.Features.Account.Dtos;
using FreeGency.Application.Features.Account.Mapping;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Domain.Specifications;
using FreeGency.Infrastructure.Integrations.Cloudinary;

namespace FreeGency.Application.Features.Account.Queries;

public partial class AccountService
{
    public async Task<Result> UpdateClientProfileAsync(UpdateClientAccountDto dto)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty) return Result.Failure(UserErrors.UserNotFound);
        var repo = unitOfWork.Repository<IClientProfileRepository, ClientProfile>();
        var spec = new ClientAccountSpecifiaction(userId);
        var clientAccount = await repo.GetEntityWithSpec(spec);
        if (clientAccount == null) return Result.Failure<ClientAccountResponseDto>(UserErrors.UserNotFound);
        clientAccount.UpdateToEntity(dto);
        if (dto.ProfileImage != null)
        {
            try
            {
                clientAccount.ProfileImage = (await storageService.UploadAsync(
                    dto.ProfileImage,
                    StorageFolders.ClientProfile)).Url;
            }
            catch (Exception)
            {
                return Result.Failure(FileErrors.UploadFailed);
            }
        }

        repo.Update(clientAccount);
        await unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> CreateProfileClientAsync()
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty) return Result.Failure(UserErrors.UserNotFound);
        var spec = new ClientAccountSpecifiaction(userId, false);
        var profile = await _profileRepository.GetEntityWithSpec(spec);
        if (profile != null) return Result.Failure(ProfileErrors.ClientProfileAlreadyExists);
        var clientProfile = new ClientProfile { UserId = userId };
        await _profileRepository.AddAsync(clientProfile);
        await unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> CreateProfileDeveloperAsync()
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty) return Result.Failure(UserErrors.UserNotFound);
        var spec = new DeveloperAccountSpecification(userId);
        var profile = await _developerProfileRepository.GetEntityWithSpec(spec);
        if (profile != null) return Result.Failure(ProfileErrors.DeveloperProfileAlreadyExists);
        var developerProfile = new DeveloperProfile { UserId = userId };
        await _developerProfileRepository.AddAsync(developerProfile);
        await unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> CompleteOnboardingAsync()
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty) return Result.Failure(UserErrors.UserNotFound);

        var user = await _userRepository.GetEntityWithSpec(new UserSpecification(userId));
        if (user is null) return Result.Failure(UserErrors.UserNotFound);
        if (user.HasCompletedOnboarding) return Result.Success();

        user.HasCompletedOnboarding = true;
        user.UpdatedAt = DateTime.UtcNow;
        _userRepository.Update(user);
        await unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<string>> SwitchModeAsync()
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty) return Result.Failure<string>(UserErrors.UserNotFound);
        var spec = new UserSpecification(userId);
        var user = await _userRepository.GetEntityWithSpec(spec);
        if (user == null) return Result.Failure<string>(UserErrors.UserNotFound);
        if (user.ActiveProfileMode == profileMode.Developer)
        {
            user.ActiveProfileMode = profileMode.Client;
            var specClient = new ClientAccountSpecifiaction(userId);
            var profile = await _profileRepository.GetEntityWithSpec(specClient);
            if (profile == null)
            {
                var clientProfile = new ClientProfile { UserId = userId };
                await _profileRepository.AddAsync(clientProfile);
                await unitOfWork.SaveChangesAsync();
            }
        }
        else
        {
            user.ActiveProfileMode = profileMode.Developer;
            var specDev = new DeveloperAccountSpecification(userId);
            var profile = await _developerProfileRepository.GetEntityWithSpec(specDev);
            if (profile == null)
            {
                var developerProfile = new DeveloperProfile { UserId = userId };
                await _developerProfileRepository.AddAsync(developerProfile);
                await unitOfWork.SaveChangesAsync();
            }
        }
        await unitOfWork.SaveChangesAsync();
        return Result.Success(user.ActiveProfileMode.ToString()!);
    }

    public Task<ApiResponse> AddClientInterestsAsync(ProfileInterestsDto dto, CancellationToken ct = default)
        => AddInterestsAsync(requireDeveloperProfile: false, dto, ct);

    public Task<ApiResponse> ReplaceClientInterestsAsync(ProfileInterestsDto dto, CancellationToken ct = default)
        => ReplaceInterestsAsync(requireDeveloperProfile: false, dto, ct);

    public Task<ApiResponse> AddDeveloperInterestsAsync(ProfileInterestsDto dto, CancellationToken ct = default)
        => AddInterestsAsync(requireDeveloperProfile: true, dto, ct);

    public Task<ApiResponse> ReplaceDeveloperInterestsAsync(ProfileInterestsDto dto, CancellationToken ct = default)
        => ReplaceInterestsAsync(requireDeveloperProfile: true, dto, ct);

    public Task<ApiResponse> AddClientSpecialtiesAsync(ProfileSpecialtiesDto dto, CancellationToken ct = default)
        => AddSpecialtiesAsync(requireDeveloperProfile: false, dto, ct);

    public Task<ApiResponse> ReplaceClientSpecialtiesAsync(ProfileSpecialtiesDto dto, CancellationToken ct = default)
        => ReplaceSpecialtiesAsync(requireDeveloperProfile: false, dto, ct);

    public Task<ApiResponse> AddDeveloperSpecialtiesAsync(ProfileSpecialtiesDto dto, CancellationToken ct = default)
        => AddSpecialtiesAsync(requireDeveloperProfile: true, dto, ct);

    public Task<ApiResponse> ReplaceDeveloperSpecialtiesAsync(ProfileSpecialtiesDto dto, CancellationToken ct = default)
        => ReplaceSpecialtiesAsync(requireDeveloperProfile: true, dto, ct);

    public Task<ApiResponse> ReplaceClientSkillsAsync(ProfileSkillsDto dto, CancellationToken ct = default)
        => ReplaceSkillsAsync(requireDeveloperProfile: false, dto, ct);

    public Task<ApiResponse> ReplaceDeveloperSkillsAsync(ProfileSkillsDto dto, CancellationToken ct = default)
        => ReplaceSkillsAsync(requireDeveloperProfile: true, dto, ct);

    private async Task<ApiResponse> AddInterestsAsync(
        bool requireDeveloperProfile,
        ProfileInterestsDto dto,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return ApiResponse.Failure(AppError.Unauthorized());

        if (!await ProfileExistsAsync(userId, requireDeveloperProfile, ct))
            return ApiResponse.Failure(AppError.NotFound(
                requireDeveloperProfile ? nameof(DeveloperProfile) : nameof(ClientProfile),
                userId));

        var categoryIds = dto.CategoryIds.Distinct().ToList();
        if (categoryIds.Count == 0)
            return ApiResponse.Failure(AppError.Validation("At least one category is required."));

        var validationError = await ValidateCategoriesAsync(categoryIds, ct);
        if (validationError is not null)
            return validationError;

        if (requireDeveloperProfile)
            await _developerProfileRepository.AddInterestsAsync(userId, categoryIds, ct);
        else
            await _profileRepository.AddInterestsAsync(userId, categoryIds, ct);

        return ApiResponse.Success("Interests added successfully.");
    }

    private async Task<ApiResponse> ReplaceInterestsAsync(
        bool requireDeveloperProfile,
        ProfileInterestsDto dto,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return ApiResponse.Failure(AppError.Unauthorized());

        if (!await ProfileExistsAsync(userId, requireDeveloperProfile, ct))
            return ApiResponse.Failure(AppError.NotFound(
                requireDeveloperProfile ? nameof(DeveloperProfile) : nameof(ClientProfile),
                userId));

        var categoryIds = dto.CategoryIds.Distinct().ToList();
        var validationError = await ValidateCategoriesAsync(categoryIds, ct);
        if (validationError is not null)
            return validationError;

        if (requireDeveloperProfile)
            await _developerProfileRepository.ReplaceInterestsAsync(userId, categoryIds, ct);
        else
            await _profileRepository.ReplaceInterestsAsync(userId, categoryIds, ct);

        return ApiResponse.Success("Interests updated successfully.");
    }

    private async Task<ApiResponse> AddSpecialtiesAsync(
        bool requireDeveloperProfile,
        ProfileSpecialtiesDto dto,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return ApiResponse.Failure(AppError.Unauthorized());

        if (!await ProfileExistsAsync(userId, requireDeveloperProfile, ct))
            return ApiResponse.Failure(AppError.NotFound(
                requireDeveloperProfile ? nameof(DeveloperProfile) : nameof(ClientProfile),
                userId));

        var specialtyIds = dto.SpecialtyIds.Distinct().ToList();
        if (specialtyIds.Count == 0)
            return ApiResponse.Failure(AppError.Validation("At least one specialty is required."));

        var validationError = await ValidateSpecialtiesAsync(specialtyIds, ct);
        if (validationError is not null)
            return validationError;

        if (requireDeveloperProfile)
            await _developerProfileRepository.AddSpecialtiesAsync(userId, specialtyIds, ct);
        else
            await _profileRepository.AddSpecialtiesAsync(userId, specialtyIds, ct);

        return ApiResponse.Success("Specialties added successfully.");
    }

    private async Task<ApiResponse> ReplaceSpecialtiesAsync(
        bool requireDeveloperProfile,
        ProfileSpecialtiesDto dto,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return ApiResponse.Failure(AppError.Unauthorized());

        if (!await ProfileExistsAsync(userId, requireDeveloperProfile, ct))
            return ApiResponse.Failure(AppError.NotFound(
                requireDeveloperProfile ? nameof(DeveloperProfile) : nameof(ClientProfile),
                userId));

        var specialtyIds = dto.SpecialtyIds.Distinct().ToList();
        var validationError = await ValidateSpecialtiesAsync(specialtyIds, ct);
        if (validationError is not null)
            return validationError;

        if (requireDeveloperProfile)
            await _developerProfileRepository.ReplaceSpecialtiesAsync(userId, specialtyIds, ct);
        else
            await _profileRepository.ReplaceSpecialtiesAsync(userId, specialtyIds, ct);

        return ApiResponse.Success("Specialties updated successfully.");
    }

    private async Task<ApiResponse> ReplaceSkillsAsync(
        bool requireDeveloperProfile,
        ProfileSkillsDto dto,
        CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty)
            return ApiResponse.Failure(AppError.Unauthorized());

        if (!await ProfileExistsAsync(userId, requireDeveloperProfile, ct))
            return ApiResponse.Failure(AppError.NotFound(
                requireDeveloperProfile ? nameof(DeveloperProfile) : nameof(ClientProfile),
                userId));

        var skillIds = dto.SkillIds.Distinct().ToList();
        var validationError = await ValidateSkillsAsync(skillIds, ct);
        if (validationError is not null)
            return validationError;

        if (requireDeveloperProfile)
            await _developerProfileRepository.ReplaceSkillsAsync(userId, skillIds, ct);
        else
            await _profileRepository.ReplaceSkillsAsync(userId, skillIds, ct);

        return ApiResponse.Success("Skills updated successfully.");
    }

    private async Task<bool> ProfileExistsAsync(Guid userId, bool developerProfile, CancellationToken ct)
    {
        if (developerProfile)
            return await _developerProfileRepository.ExistsForUserAsync(userId, ct);

        return await _profileRepository.ExistsForUserAsync(userId, ct);
    }

    private async Task<ApiResponse?> ValidateCategoriesAsync(IReadOnlyList<Guid> categoryIds, CancellationToken ct)
    {
        var categoryRepository = unitOfWork.Repository<ICategoryRepository, Category>();

        foreach (var categoryId in categoryIds)
        {
            if (!await categoryRepository.ExistsAsync(categoryId, ct))
                return ApiResponse.Failure(AppError.NotFound(nameof(Category), categoryId));
        }

        return null;
    }

    private async Task<ApiResponse?> ValidateSpecialtiesAsync(IReadOnlyList<Guid> specialtyIds, CancellationToken ct)
    {
        var specialtyRepository = unitOfWork.Repository<ISpecialtyRepository, Specialty>();

        foreach (var specialtyId in specialtyIds)
        {
            if (!await specialtyRepository.ExistsAsync(specialtyId, ct))
                return ApiResponse.Failure(AppError.NotFound(nameof(Specialty), specialtyId));
        }

        return null;
    }

    private async Task<ApiResponse?> ValidateSkillsAsync(IReadOnlyList<Guid> skillIds, CancellationToken ct)
    {
        var skillRepository = unitOfWork.Repository<ISkillRepository, Skill>();

        foreach (var skillId in skillIds)
        {
            if (!await skillRepository.ExistsAsync(skillId, ct))
                return ApiResponse.Failure(AppError.NotFound(nameof(Skill), skillId));
        }

        return null;
    }
}
