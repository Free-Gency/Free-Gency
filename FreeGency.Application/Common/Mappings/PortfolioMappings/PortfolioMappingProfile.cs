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

            CreateMap<PortfolioRoadmapStep, PortfolioRoadmapStepDto>();
            CreateMap<PortfolioMetric, PortfolioMetricDto>();

            CreateMap<CreatePortfolioProjectRequestDto, PortfolioProject>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.OwnerUserId, o => o.Ignore())
                .ForMember(d => d.ImageCover, o => o.Ignore())
                .ForMember(d => d.PortfolioImages, o => o.Ignore())
                .ForMember(d => d.PortfolioSkills, o => o.Ignore())
                .ForMember(d => d.RoadmapSteps, o => o.Ignore())
                .ForMember(d => d.Metrics, o => o.Ignore())
                .ForMember(d => d.Feedbacks, o => o.Ignore())
                .ForMember(d => d.RecentlyViewedByUsers, o => o.Ignore())
                .ForMember(d => d.OwnerUser, o => o.Ignore())
                .ForMember(d => d.OwnerTeam, o => o.Ignore())
                .ForMember(d => d.Category, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.CreatedBy, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedBy, o => o.Ignore())
                .ForMember(d => d.IsDeleted, o => o.Ignore())
                .ForMember(d => d.DeletedAt, o => o.Ignore())
                .ForMember(d => d.DeletedBy, o => o.Ignore());

            CreateMap<UpdatePortfolioProjectRequestDto, PortfolioProject>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.OwnerType, o => o.Ignore())
                .ForMember(d => d.OwnerUserId, o => o.Ignore())
                .ForMember(d => d.OwnerTeamId, o => o.Ignore())
                .ForMember(d => d.ImageCover, o => o.Ignore())
                .ForMember(d => d.PortfolioImages, o => o.Ignore())
                .ForMember(d => d.PortfolioSkills, o => o.Ignore())
                .ForMember(d => d.RoadmapSteps, o => o.Ignore())
                .ForMember(d => d.Metrics, o => o.Ignore())
                .ForMember(d => d.Feedbacks, o => o.Ignore())
                .ForMember(d => d.RecentlyViewedByUsers, o => o.Ignore())
                .ForMember(d => d.OwnerUser, o => o.Ignore())
                .ForMember(d => d.OwnerTeam, o => o.Ignore())
                .ForMember(d => d.Category, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.CreatedBy, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedBy, o => o.Ignore())
                .ForMember(d => d.IsDeleted, o => o.Ignore())
                .ForMember(d => d.DeletedAt, o => o.Ignore())
                .ForMember(d => d.DeletedBy, o => o.Ignore());

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
                    o => o.MapFrom(s => s.PortfolioSkills))
                .ForMember(d => d.RoadmapSteps,
                    o => o.MapFrom(s => s.RoadmapSteps.OrderBy(r => r.SortOrder)))
                .ForMember(d => d.Metrics,
                    o => o.MapFrom(s => s.Metrics.OrderBy(m => m.SortOrder)))
                .ForMember(d => d.Creator, o => o.Ignore())
                .ForMember(d => d.OwnerReviews, o => o.Ignore());
        }
    }
}
