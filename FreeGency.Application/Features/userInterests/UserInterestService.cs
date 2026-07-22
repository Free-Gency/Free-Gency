using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Common.Mappings.CategoriesMapping;
using FreeGency.Application.Features.categories.Dtos;
using FreeGency.Application.Features.userInterests.Dtos;
using FreeGency.Infrastructure.Interfaces;

namespace FreeGency.Application.Features.userInterests;

public class UserInterestService : IUserInterestService
{
    private readonly IDeveloperProfileRepository _developerProfileRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UserInterestService(ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _developerProfileRepository = _unitOfWork.Repository<IDeveloperProfileRepository, DeveloperProfile>();
        _categoryRepository = _unitOfWork.Repository<ICategoryRepository, Category>();
    }

    public async Task<ApiResponse<IEnumerable<CategoryDto>>> GetMyInterestsAsync(CancellationToken ct = default)
    {
        var profile = await _developerProfileRepository.GetByUserIdWithSkillsAndInterestsAsync(_currentUser.UserId, ct);

        if (profile is null)
            return ApiResponse.Failure<IEnumerable<CategoryDto>>(AppError.NotFound(nameof(DeveloperProfile), _currentUser.UserId));

        var interests = profile.User.UserInterests
            .Select(ui => ui.Category.ToDto())
            .ToList();

        return ApiResponse.Success<IEnumerable<CategoryDto>>(interests);
    }

    public async Task<ApiResponse> ReplaceMyInterestsAsync(ReplaceUserInterestsDto dto, CancellationToken ct = default)
    {
        if (!await _developerProfileRepository.ExistsForUserAsync(_currentUser.UserId, ct))
            return ApiResponse.Failure(AppError.NotFound(nameof(DeveloperProfile), _currentUser.UserId));

        var categoryIds = dto.CategoryIds.Distinct().ToList();

        foreach (var categoryId in categoryIds)
        {
            if (!await _categoryRepository.ExistsAsync(categoryId, ct))
                return ApiResponse.Failure(AppError.NotFound(nameof(Category), categoryId));
        }

        await _developerProfileRepository.ReplaceInterestsAsync(_currentUser.UserId, categoryIds, ct);

        return ApiResponse.Success("User interests updated successfully.");
    }
}
