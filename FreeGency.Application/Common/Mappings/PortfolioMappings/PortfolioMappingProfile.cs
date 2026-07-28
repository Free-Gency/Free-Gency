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
                        x.Category != null ? x.Category.NameEn : null,
                        x.OwnerTeam != null
                            ? x.OwnerTeam.Name
                            : x.OwnerUser != null
                                ? $"{x.OwnerUser.FristName} {x.OwnerUser.LastName}".Trim()
                                : null))
                // Prevent AutoMapper flattening Category.Name (Arabic) over NameEn
                .ForMember(
                    d => d.CategoryName,
                    o => o.MapFrom(s => s.Category != null ? s.Category.NameEn : null))
                .ForMember(
                    d => d.OwnerName,
                    o => o.MapFrom(s =>
                        s.OwnerTeam != null
                            ? s.OwnerTeam.Name
                            : s.OwnerUser != null
                                ? $"{s.OwnerUser.FristName} {s.OwnerUser.LastName}".Trim()
                                : null));

            CreateMap<PortfolioProject, PortfolioProjectDetailsDto>()
                .ForMember(d => d.CategoryName,
                    o => o.MapFrom(s => s.Category != null ? s.Category.NameEn : null))
                .ForMember(d => d.OwnerName,
                    o => o.MapFrom(s =>
                        s.OwnerTeam != null
                            ? s.OwnerTeam.Name
                            : s.OwnerUser != null
                                ? $"{s.OwnerUser.FristName} {s.OwnerUser.LastName}".Trim()
                                : null))
                .ForMember(d => d.Images,
                    o => o.MapFrom(s => s.PortfolioImages.OrderBy(i => i.SortOrder)))
                .ForMember(d => d.Skills,
                    o => o.MapFrom(s => s.PortfolioSkills));
        }
    }
}
