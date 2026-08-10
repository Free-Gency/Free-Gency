using FreeGency.Application.Features.Milestones.DTOs;

namespace FreeGency.Application.Common.Mappings.MilestoneMappings;

public class MilestoneMapping : Profile
{
    public MilestoneMapping()
    {
        CreateMap<Milestone, MilestoneDto>()
            .ForMember(dest => dest.ReleaseStatus, opt => opt.MapFrom(src => src.ReleaseStatus.ToString()))
            .ForMember(dest => dest.WorkStatus, opt => opt.MapFrom(src => src.WorkStatus.ToString()))
            .ForMember(dest => dest.ProposedByUserId, opt => opt.MapFrom(src => ParseGuidOrNull(src.ProposedByUserId)))
            .ForMember(dest => dest.Files, opt => opt.MapFrom(src =>
                src.ProjectFiles.Select(f => new MilestoneFileDto
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    FileUrl = f.FileUrl,
                    FileKind = f.FileKind.ToString(),
                    CreatedAt = f.CreatedAt
                })));

        CreateMap<Milestone, DeveloperMilestoneDto>()
            .ForMember(dest => dest.ReleaseStatus, opt => opt.MapFrom(src => src.ReleaseStatus.ToString()))
            .ForMember(dest => dest.WorkStatus, opt => opt.MapFrom(src => src.WorkStatus.ToString()))
            .ForMember(dest => dest.ProjectTitle, opt => opt.MapFrom(src => src.Project != null ? src.Project.Title : string.Empty))
            .ForMember(dest => dest.ProjectStatus, opt => opt.MapFrom(src => src.Project != null ? src.Project.Status.ToString() : string.Empty));
    }

    private static Guid? ParseGuidOrNull(string? value)
        => Guid.TryParse(value, out var id) ? id : null;
}