using FreeGency.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Domain.Specifications
{
    public class DeveloperAccountSpecification:BaseSpecification<DeveloperProfile>
    {
        public DeveloperAccountSpecification(Guid userId):base(x=>x.UserId==userId)
        {
            
        }
        public DeveloperAccountSpecification(Guid userId,bool includes) : base(x => x.UserId == userId)
        {
            AddInclude("User");

            AddInclude("User.UserSpecialties");

            AddInclude("User.UserSpecialties.Specialty");

            AddInclude("User.UserSpecialties.Specialty.SpecialtySkills");

            AddInclude("User.UserSpecialties.Specialty.SpecialtySkills.Skill");
        }
    }
}
