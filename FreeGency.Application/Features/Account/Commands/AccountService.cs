using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Models;
using FreeGency.Application.Features.Account.Dtos;
using FreeGency.Application.Features.Account.Mapping;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Domain.Specifications;
using FreeGency.Infrastructure.Integrations.Cloudinary;
using Microsoft.AspNetCore.Identity;

namespace FreeGency.Application.Features.Account.Queries;

public partial class AccountService
{
    private readonly IClientNotificationSettingsRepository clientNotificationSettingsRepository = unitOfWork.Repository<IClientNotificationSettingsRepository, ClientNotificationSettings>();
    private readonly IDeveloperNotificationSettingsRepository developerNotificationSettingsRepository = unitOfWork.Repository<IDeveloperNotificationSettingsRepository, DeveloperNotificationSettings>();
    public async Task<Result> ChangePasswordAsync(ChangePasswordRequestDto dto)
    {
        var userId =currentUserService.UserId;

        if (userId ==Guid.Empty)
            return Result.Failure(UserErrors.UserNotFound);
        var user = await userManager.FindByIdAsync(userId.ToString());
        var result = await userManager.ChangePasswordAsync(
            user!,
            dto.CurrentPassword,
            dto.NewPassword);

        if (!result.Succeeded)
        {
            return Result.Failure(new Error ( result.Errors.First().Code, result.Errors.First().Description, StatusCodes.Status409Conflict ));
        }

        return Result.Success();
    }
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

    public async Task<Result> UpdateDeveloperProfileAsync(UpdateDeveloperAccountDto dto)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty) return Result.Failure(UserErrors.UserNotFound);
        var repo = unitOfWork.Repository<IDeveloperProfileRepository, DeveloperProfile>();
        var spec = new DeveloperAccountSpecification(userId, true);
        var developerAccount = await repo.GetEntityWithSpec(spec);
        if (developerAccount == null) return Result.Failure(UserErrors.UserNotFound);
        developerAccount.UpdateToEntity(dto);
        if (dto.ProfileImage != null)
        {
            try
            {
                developerAccount.ProfileImage = (await storageService.UploadAsync(
                    dto.ProfileImage,
                    StorageFolders.ClientProfile)).Url;
            }
            catch (Exception)
            {
                return Result.Failure(FileErrors.UploadFailed);
            }
        }

        repo.Update(developerAccount);
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
        var clientProfile = new ClientProfile { Id=Guid.NewGuid(),UserId = userId };
        await _profileRepository.AddAsync(clientProfile);
        await clientNotificationSettingsRepository.AddAsync(new ClientNotificationSettings { Id = Guid.NewGuid(), ProfileId = clientProfile.Id });
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
        var developerProfile = new DeveloperProfile { Id = Guid.NewGuid(), UserId = userId };
        await _developerProfileRepository.AddAsync(developerProfile);
        await developerNotificationSettingsRepository.AddAsync(new DeveloperNotificationSettings { Id = Guid.NewGuid(), ProfileId = developerProfile.Id });
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

    public async Task<Result<ProfileModesDto>> GetProfileModesAsync()
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty) return Result.Failure<ProfileModesDto>(UserErrors.UserNotFound);

        var user = await _userRepository.GetEntityWithSpec(new UserSpecification(userId));
        if (user is null) return Result.Failure<ProfileModesDto>(UserErrors.UserNotFound);

        var hasClient = await _profileRepository.GetEntityWithSpec(new ClientAccountSpecifiaction(userId, false)) is not null;
        var hasDeveloper = await _developerProfileRepository.GetEntityWithSpec(new DeveloperAccountSpecification(userId)) is not null;

        Guid? activeProfileId = null;
        if (user.ActiveProfileMode == profileMode.Client && hasClient)
        {
            var client = await _profileRepository.GetEntityWithSpec(new ClientAccountSpecifiaction(userId, false));
            activeProfileId = client?.Id;
        }
        else if (user.ActiveProfileMode == profileMode.Developer && hasDeveloper)
        {
            var developer = await _developerProfileRepository.GetEntityWithSpec(new DeveloperAccountSpecification(userId));
            activeProfileId = developer?.Id;
        }

        return Result.Success(new ProfileModesDto
        {
            ActiveProfileMode = user.ActiveProfileMode?.ToString() ?? string.Empty,
            HasClientProfile = hasClient,
            HasDeveloperProfile = hasDeveloper,
            ActiveProfileId = activeProfileId,
        });
    }

    public async Task<Result<SwitchProfileResponseDto>> SwitchModeAsync(string? targetMode = null)
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty) return Result.Failure<SwitchProfileResponseDto>(UserErrors.UserNotFound);
        var spec = new UserSpecification(userId);
        var user = await _userRepository.GetEntityWithSpec(spec);
        if (user == null) return Result.Failure<SwitchProfileResponseDto>(UserErrors.UserNotFound);

        var hasClient = await _profileRepository.GetEntityWithSpec(new ClientAccountSpecifiaction(userId, false)) is not null;
        var hasDeveloper = await _developerProfileRepository.GetEntityWithSpec(new DeveloperAccountSpecification(userId)) is not null;

        profileMode nextMode;
        if (!string.IsNullOrWhiteSpace(targetMode))
        {
            nextMode = targetMode.Equals("Developer", StringComparison.OrdinalIgnoreCase)
                ? profileMode.Developer
                : profileMode.Client;
        }
        else
        {
            nextMode = user.ActiveProfileMode == profileMode.Developer
                ? profileMode.Client
                : profileMode.Developer;
        }

        if (nextMode == user.ActiveProfileMode)
        {
            Guid? currentId = null;
            if (nextMode == profileMode.Client && hasClient)
                currentId = (await _profileRepository.GetEntityWithSpec(new ClientAccountSpecifiaction(userId, false)))?.Id;
            else if (nextMode == profileMode.Developer && hasDeveloper)
                currentId = (await _developerProfileRepository.GetEntityWithSpec(new DeveloperAccountSpecification(userId)))?.Id;

            return Result.Success(new SwitchProfileResponseDto
            {
                ActiveProfileMode = nextMode.ToString(),
                ProfileId = currentId,
                HasClientProfile = hasClient,
                HasDeveloperProfile = hasDeveloper,
            });
        }

        if (nextMode == profileMode.Client)
        {
            if (!hasClient)
                return Result.Failure<SwitchProfileResponseDto>(ProfileErrors.ClientProfileRequired);

            user.ActiveProfileMode = profileMode.Client;
            var client = await _profileRepository.GetEntityWithSpec(new ClientAccountSpecifiaction(userId, false));
            _userRepository.Update(user);
            await unitOfWork.SaveChangesAsync();

            return Result.Success(new SwitchProfileResponseDto
            {
                ActiveProfileMode = nameof(profileMode.Client),
                ProfileId = client?.Id,
                HasClientProfile = true,
                HasDeveloperProfile = hasDeveloper,
            });
        }

        if (!hasDeveloper)
            return Result.Failure<SwitchProfileResponseDto>(ProfileErrors.DeveloperProfileRequired);

        user.ActiveProfileMode = profileMode.Developer;
        var developer = await _developerProfileRepository.GetEntityWithSpec(new DeveloperAccountSpecification(userId));
        _userRepository.Update(user);
        await unitOfWork.SaveChangesAsync();

        return Result.Success(new SwitchProfileResponseDto
        {
            ActiveProfileMode = nameof(profileMode.Developer),
            ProfileId = developer?.Id,
            HasClientProfile = hasClient,
            HasDeveloperProfile = true,
        });
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
