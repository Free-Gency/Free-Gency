using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Specifications;

public class DeveloperAccountSpecification : BaseSpecification<DeveloperProfile>
{
    public DeveloperAccountSpecification(Guid userId) : base(x => x.UserId == userId)
    {
    }

    public DeveloperAccountSpecification(Guid userId, bool includes) : base(x => x.UserId == userId)
    {
        AddInclude("User");
        AddInclude("UserSpecialties.Specialty");
        AddInclude("UserSpecialties.Specialty.CategorySpecialties");
        AddInclude("UserInterests.Category");
        AddInclude("UserSkills.Skill");
        AddInclude("UserSkills.Skill.SpecialtySkills");
    }
}
