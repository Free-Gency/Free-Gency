using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Common.Mappings.SpecialtiesMapping;
using FreeGency.Application.Features.specialties.Dtos;

namespace FreeGency.Application.Features.specialties.Commands
{
    // Commands
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

        public async Task<ApiResponse<Guid>> CreateAsync(CreateSpecialtyDto dto, CancellationToken ct = default)
        {
            var specialty = dto.ToEntity();

            await _specialtyRepository.AddAsync(specialty, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse.Success(specialty.Id, "Specialty created successfully.");
        }

        public async Task<ApiResponse> UpdateAsync(UpdateSpecialtyDto dto, CancellationToken ct = default)
        {
            var specialty = await _specialtyRepository.GetByIdAsync(dto.Id, ct);

            if (specialty is null)
                return ApiResponse.Failure(AppError.NotFound(nameof(Specialty), dto.Id));

            specialty.NameEn = dto.NameEn;
            specialty.NameAr = dto.NameAr;

            _specialtyRepository.Update(specialty);
            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse.Success("Specialty updated successfully.");
        }

        public async Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var specialty = await _specialtyRepository.GetByIdAsync(id, ct);

            if (specialty is null)
                return ApiResponse.Failure(AppError.NotFound(nameof(Specialty), id));

            _specialtyRepository.Delete(specialty);
            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse.Success("Specialty deleted successfully.");
        }
    }
}
