using FreeGency.Application.Common.Errors;
using FreeGency.Application.Features.Account.Dtos;
using FreeGency.Application.Features.Account.Mapping;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Domain.Specifications;

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
            clientAccount.ProfileImage = await SaveImage(dto.ProfileImage, "ClientProfile");
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
    public async Task<Result<string>> SwitchModeAsync()
    {
        var userId = currentUserService.UserId;
        if (userId == Guid.Empty) return Result.Failure<string>(UserErrors.UserNotFound);
        var spec = new UserSpecification(userId);
        var user = await _userRepository.GetEntityWithSpec(spec);
        if (user == null) return Result.Failure<string>(UserErrors.UserNotFound);
        if (user.ActiveProfileMode == profileMode.Client)
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
        else
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
        var validationError = await ValidateCategoriesAsync(categoryIds, ct);
        if (validationError is not null)
            return validationError;

        var developerProfileRepository = unitOfWork.Repository<IDeveloperProfileRepository, DeveloperProfile>();
        await developerProfileRepository.AddInterestsAsync(userId, categoryIds, ct);

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

        var developerProfileRepository = unitOfWork.Repository<IDeveloperProfileRepository, DeveloperProfile>();
        await developerProfileRepository.ReplaceInterestsAsync(userId, categoryIds, ct);

        return ApiResponse.Success("Interests updated successfully.");
    }

    private async Task<bool> ProfileExistsAsync(Guid userId, bool developerProfile, CancellationToken ct)
    {
        if (developerProfile)
        {
            var developerProfileRepository = unitOfWork.Repository<IDeveloperProfileRepository, DeveloperProfile>();
            return await developerProfileRepository.ExistsForUserAsync(userId, ct);
        }

        var clientProfileRepository = unitOfWork.Repository<IClientProfileRepository, ClientProfile>();
        return await clientProfileRepository.ExistsForUserAsync(userId, ct);
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

    private async Task<string> SaveImage(IFormFile file, string type)
    {
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/Images/{type}");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);
        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }
        return $"/Images/{type}/{uniqueFileName}";
    }
}
