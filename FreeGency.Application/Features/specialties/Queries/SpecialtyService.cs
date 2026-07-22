using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Mappings.SpecialtiesMapping;
using FreeGency.Application.Features.specialties.Dtos;

namespace FreeGency.Application.Features.specialties;

public partial class SpecialtyService
{
    public async Task<Result<IEnumerable<SpecialtyDto>>> GetAllAsync( CancellationToken ct = default)
    {
        var specialties = await _specialtyRepository.GetAllAsync(ct);

        return Result.Success(specialties.Select(x => x.ToDto()));
    }

    public async Task<Result<SpecialtyDto>> GetByIdAsync( Guid id,CancellationToken ct = default)
    {
        var specialty = await _specialtyRepository.GetByIdAsync(id, ct);

        if (specialty is null)
            return Result.Failure<SpecialtyDto>(SpecialtyErrors.NotFound);

        return Result.Success(specialty.ToDto());
    }

    public async Task<Result<IEnumerable<SpecialtyDto>>> GetByCategoryIdAsync(Guid categoryId,CancellationToken ct = default)
    {
        var specialties =
            await _specialtyRepository.GetByCategoryIdAsync(categoryId, ct);

        return Result.Success(
            specialties.Select(x => x.ToDto()));
    }
}