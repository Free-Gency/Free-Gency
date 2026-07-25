using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Common.Mappings.CategoriesMapping;
using FreeGency.Application.Features.categories.Dtos;
using FreeGency.Infrastructure.Integrations.Cloudinary;
using FreeGency.Infrastructure.Interfaces;

namespace FreeGency.Application.Features.categories.Commands
{
    // Commands
    public partial class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISpecialtyRepository _specialtyRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStorageService _storageService;

        public CategoryService(IUnitOfWork unitOfWork, IStorageService storageService)
        {
            _unitOfWork = unitOfWork;
            _storageService = storageService;
            _categoryRepository = _unitOfWork.Repository<ICategoryRepository, Category>();
            _specialtyRepository = _unitOfWork.Repository<ISpecialtyRepository, Specialty>();
            _skillRepository = _unitOfWork.Repository<ISkillRepository, Skill>();
        }

        public async Task<ApiResponse<Guid>> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default)
        {
            if (await _categoryRepository.ExistsByNameAsync(dto.Name, ct: ct))
                return ApiResponse.Failure<Guid>(AppError.CategoryNameAlreadyExists(dto.Name));

            string? imageCoverUrl = null;
            if (dto.ImageCover is not null)
            {
                try
                {
                    imageCoverUrl = (await _storageService.UploadAsync(dto.ImageCover, StorageFolders.Category, ct)).Url;
                }
                catch (Exception)
                {
                    return ApiResponse.Failure<Guid>(AppError.FileUploadFailed(dto.ImageCover.FileName));
                }
            }

            var category = dto.ToEntity(imageCoverUrl);

            await _categoryRepository.AddAsync(category, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse.Success(category.Id, "Category created successfully.");
        }

        public async Task<ApiResponse> UpdateAsync(UpdateCategoryDto dto, CancellationToken ct = default)
        {
            var category = await _categoryRepository.GetByIdAsync(dto.Id, ct);

            if (category is null)
                return ApiResponse.Failure(AppError.NotFound(nameof(Category), dto.Id));

            if (await _categoryRepository.ExistsByNameAsync(dto.Name, dto.Id, ct))
                return ApiResponse.Failure(AppError.CategoryNameAlreadyExists(dto.Name));

            category.Name = dto.Name;
            category.NameEn = dto.NameEn;

            if (dto.ImageCover is not null)
            {
                try
                {
                    category.ImageCover = (await _storageService.UploadAsync(dto.ImageCover, StorageFolders.Category, ct)).Url;
                }
                catch (Exception)
                {
                    return ApiResponse.Failure(AppError.FileUploadFailed(dto.ImageCover.FileName));
                }
            }

            _categoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse.Success("Category updated successfully.");
        }

        public async Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var category = await _categoryRepository.GetByIdAsync(id, ct);

            if (category is null)
                return ApiResponse.Failure(AppError.NotFound(nameof(Category), id));

            _categoryRepository.Delete(category);
            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse.Success("Category deleted successfully.");
        }
    }
}
