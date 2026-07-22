using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Mappings.SpecialtiesMapping;
using FreeGency.Application.Features.specialties.Dtos;

namespace FreeGency.Application.Features.specialties;

public partial class SpecialtyService
{
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
