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
        AddInclude("UserSpecialties.Specialty.SpecialtySkills");
        AddInclude("UserSpecialties.Specialty.SpecialtySkills.Skill");
        AddInclude("UserInterests.Category");
        AddInclude("UserSkills.Skill");
    }
}
