using FreeGency.Application.Common.Interfaces;

namespace FreeGency.Application.Features.skills;

public partial class SkillService : ISkillService
{
    private readonly ISkillRepository _skillRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISpecialtyRepository _specialtyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SkillService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _skillRepository = _unitOfWork.Repository<ISkillRepository, Skill>();
        _categoryRepository = _unitOfWork.Repository<ICategoryRepository, Category>();
        _specialtyRepository = _unitOfWork.Repository<ISpecialtyRepository, Specialty>();
    }
}
