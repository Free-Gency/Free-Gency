using FreeGency.Application.Common.Interfaces;

namespace FreeGency.Application.Features.specialties;

public partial class SpecialtyService : ISpecialtyService
{
    private readonly ISpecialtyRepository _specialtyRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISkillRepository _skillRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SpecialtyService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _specialtyRepository = _unitOfWork.Repository<ISpecialtyRepository, Specialty>();
        _categoryRepository = _unitOfWork.Repository<ICategoryRepository, Category>();
        _skillRepository = _unitOfWork.Repository<ISkillRepository, Skill>();
    }
}
