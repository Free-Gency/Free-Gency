namespace FreeGency.Application.Common.Mappings.PortfolioMappings
{
    public class PortfolioMappingProfile : Profile
    {
        public PortfolioMappingProfile()
        {
            CreateMap<PortfolioImage, PortfolioImageDto>();

            CreateMap<PortfolioSkill, PortfolioSkillDto>()
                .ConstructUsing(x =>
                    new PortfolioSkillDto(
                        x.SkillId,
                        x.Skill.Name));

            CreateMap<PortfolioProject, PortfolioProjectDto>()
                .ConstructUsing(x =>
                    new PortfolioProjectDto(
                        x.Id,
                        x.Title,
                        x.Description,
                        x.Budget,
                        x.ImageCover,
                        x.ProjectUrl,
                        x.CompletionDate,
                        x.Visibility,
                        x.Category != null ? x.Category.Name : null));

            CreateMap<PortfolioProject, PortfolioProjectDetailsDto>()
                .ForMember(d => d.CategoryName,
                    o => o.MapFrom(s => s.Category != null ? s.Category.Name : null))
                .ForMember(d => d.Images,
                    o => o.MapFrom(s => s.PortfolioImages))
                .ForMember(d => d.Skills,
                    o => o.MapFrom(s => s.PortfolioSkills));
        }
    }
}
