using FreeGency.Application.Features.ProjectEvents.DTOs;


namespace FreeGency.Application.Common.Mappings.ProjectEventMappings;

public class ProjectEventMapping : Profile
{
    public ProjectEventMapping()
    {
        CreateMap<ProjectEvent, ProjectEventDto>()
            .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => src.EventType.ToString()))
            .ForMember(dest => dest.MilestoneTitle, opt => opt.MapFrom(src =>
                src.Milestone != null ? src.Milestone.Title : null))
            .ForMember(dest => dest.ActorUserName, opt => opt.MapFrom(src =>
                $"{src.ActorUser.FristName} {src.ActorUser.LastName}"));
    }
}