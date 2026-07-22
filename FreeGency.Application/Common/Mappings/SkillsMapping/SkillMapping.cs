using FreeGency.Application.Features.skills.Dtos;

namespace FreeGency.Application.Common.Mappings.SkillsMapping;

public static class SkillMapping
{
    public static SkillDto ToDto(this Skill skill)
        => new()
        {
            Id = skill.Id,
            Name = skill.Name
        };

    public static Skill ToEntity(this CreateSkillDto dto)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = dto.Name
        };
}
