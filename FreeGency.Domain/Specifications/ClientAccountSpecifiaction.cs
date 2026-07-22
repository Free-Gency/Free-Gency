using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Specifications;

public class ClientAccountSpecifiaction : BaseSpecification<ClientProfile>
{
    public ClientAccountSpecifiaction(Guid userId) : base(x => x.UserId == userId)
    {
        AddInclude("User");
        AddInclude("UserInterests.Category");
        AddInclude("UserSpecialties.Specialty");
        AddInclude("UserSpecialties.Specialty.SpecialtySkills");
        AddInclude("UserSpecialties.Specialty.SpecialtySkills.Skill");
        AddInclude("UserSkills.Skill");
    }

    public ClientAccountSpecifiaction(Guid userId, bool? includes) : base(x => x.UserId == userId)
    {
    }
}
