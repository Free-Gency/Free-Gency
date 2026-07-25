using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Specifications;

public class ClientAccountSpecifiaction : BaseSpecification<ClientProfile>
{
    public ClientAccountSpecifiaction(Guid userId) : base(x => x.UserId == userId)
    {
        AddInclude("User");
    }

    public ClientAccountSpecifiaction(Guid userId, bool? includes) : base(x => x.UserId == userId)
    {
    }

    public static ClientAccountSpecifiaction ForInterestCatalog(Guid userId)
    {
        var spec = new ClientAccountSpecifiaction(userId, includes: null);
        spec.IncludeInterestCatalog();
        return spec;
    }

    private void IncludeInterestCatalog()
    {
        AddInclude("UserInterests.Category");
        AddInclude("UserSpecialties.Specialty");
        AddInclude("UserSpecialties.Specialty.CategorySpecialties");
        AddInclude("UserSkills.Skill");
        AddInclude("UserSkills.Skill.SpecialtySkills");
    }
}
