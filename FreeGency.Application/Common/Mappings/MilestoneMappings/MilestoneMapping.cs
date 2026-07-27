using FreeGency.Application.Features.Milestones.DTOs;

namespace FreeGency.Application.Common.Mappings.MilestoneMappings;

public class MilestoneMapping : Profile
{
    public MilestoneMapping()
    {
        CreateMap<Milestone, MilestoneDto>()
            .ForMember(dest => dest.ReleaseStatus, opt => opt.MapFrom(src => src.ReleaseStatus.ToString()))
            .ForMember(dest => dest.WorkStatus, opt => opt.MapFrom(src => src.WorkStatus.ToString()))
            .ForMember(dest => dest.Files, opt => opt.MapFrom(src =>
                src.ProjectFiles.Select(f => new MilestoneFileDto
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    FileUrl = f.FileUrl,
                    FileKind = f.FileKind.ToString(),
                    CreatedAt = f.CreatedAt
                })));
    }
}