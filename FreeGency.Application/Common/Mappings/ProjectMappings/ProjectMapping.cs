namespace FreeGency.Application.Common.Mappings.ProjectMappings
{
    public class ProjectMapping : Profile
    {

        public ProjectMapping()
        {
            CreateMap<Project, ProjectDto>()
                .ForMember(dist => dist.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dist => dist.ClientName, opt => opt.MapFrom(src => $"{src.Client.FristName} {src.Client.LastName}"))
                .ForMember(dist => dist.CategoryName, opt => opt.MapFrom(src => src.Category.NameEn))
                .ForMember(dist => dist.ClientAvatarUrl, opt => opt.MapFrom(src => src.Client.ClientProfile!.ProfileImage ?? "Default"))
                .ForMember(dist => dist.Specialties, opt => opt.MapFrom(src => src.ProjectSpecialties.Select(s => s.Specialty.NameEn)))
                .ForMember(dist => dist.Skills, opt => opt.MapFrom(src => src.ProjectSkills.Select(ps => ps.Skill.Name)))
                .ForMember(dist => dist.ProposalCount, opt => opt.MapFrom(src => src.ProjectProposals.Count()))
                .ForMember(dist => dist.ClientId, opt => opt.MapFrom(src => src.ClientId))
                .ForMember(dist => dist.ClientRating, opt => opt.MapFrom(src => src.Client.ClientProfile != null ? src.Client.ClientProfile.AverageRating : 0m))
                .ForMember(dist => dist.SkillIds, opt => opt.MapFrom(src => src.ProjectSkills.Select(ps => ps.SkillId.ToString())));
        }


        //public static ProjectDto ToDto(this Project project)
        //    => new()
        //    {
        //        Id = project.Id,
        //        Title = project.Title,
        //        Description = project.Description,
        //        IsFixedPrice = project.IsFixedPrice,
        //        BudgetMin = project.BudgetMin,
        //        BudgetMax = project.BudgetMax,
        //        Currency = project.Currency,
        //        EstimatedDurationDays = project.EstimatedDurationDays,
        //        Deadline = project.Deadline,
        //        Status = project.Status.ToString(),
        //        CreatedAt = project.CreatedAt,
        //        CategoryName = project.Category.Name,
        //        ClientName = $"{project.Client.FristName} {project.Client.LastName}",
        //        ClientAvatarUrl = project.Client.ClientProfile?.ProfileImage,
        //        Specialties = project.ProjectSpecialties.Select(s => s.Specialty.NameEn),
        //        Skills = project.ProjectSkills.Select(ps => ps.Skill.Name),
        //        ProposalCount = project.ProjectProposals.Count()
        //        //IsSaved = project.
        //    };
    }
}
