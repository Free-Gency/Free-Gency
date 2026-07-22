using FreeGency.Application.Common.Interfaces;

namespace FreeGency.Application.Features.categories;

public partial class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISpecialtyRepository _specialtyRepository;
    private readonly ISkillRepository _skillRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _categoryRepository = _unitOfWork.Repository<ICategoryRepository, Category>();
        _specialtyRepository = _unitOfWork.Repository<ISpecialtyRepository, Specialty>();
        _skillRepository = _unitOfWork.Repository<ISkillRepository, Skill>();
    }
}
