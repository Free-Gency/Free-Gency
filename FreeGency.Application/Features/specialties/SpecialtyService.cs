using FreeGency.Application.Common.Interfaces;
using FreeGency.Domain.Interfaces;
using FreeGency.Domain.Interfaces.Repositories;

namespace FreeGency.Application.Features.specialties;

public partial class SpecialtyService : ISpecialtyService
{
    private readonly ISpecialtyRepository _specialtyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SpecialtyService(
        ISpecialtyRepository specialtyRepository,
        IUnitOfWork unitOfWork)
    {
        _specialtyRepository = specialtyRepository;
        _unitOfWork = unitOfWork;
    }
}