using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Mappings.SkillsMapping;
using FreeGency.Application.Features.skills.Dtos;

namespace FreeGency.Application.Features.skills;

public partial class SkillService
{
    public async Task<ApiResponse<Guid>> CreateAsync(CreateSkillDto dto, CancellationToken ct = default)
    {
        if (await _skillRepository.ExistsByNameAsync(dto.Name, ct: ct))
            return ApiResponse.Failure<Guid>(AppError.SkillNameAlreadyExists(dto.Name));

        var skill = dto.ToEntity();

        await _skillRepository.AddAsync(skill, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success(skill.Id, "Skill created successfully.");
    }

    public async Task<ApiResponse> UpdateAsync(UpdateSkillDto dto, CancellationToken ct = default)
    {
        var skill = await _skillRepository.GetByIdAsync(dto.Id, ct);

        if (skill is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Skill), dto.Id));

        if (await _skillRepository.ExistsByNameAsync(dto.Name, dto.Id, ct))
            return ApiResponse.Failure(AppError.SkillNameAlreadyExists(dto.Name));

        skill.Name = dto.Name;

        _skillRepository.Update(skill);
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success("Skill updated successfully.");
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var skill = await _skillRepository.GetByIdAsync(id, ct);

        if (skill is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Skill), id));

        _skillRepository.Delete(skill);
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success("Skill deleted successfully.");
    }
}
