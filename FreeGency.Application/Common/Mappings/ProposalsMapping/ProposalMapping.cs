
using FreeGency.Application.Features.Proposals.Dtos;
using FreeGency.Domain.Enums;

namespace FreeGency.Application.Common.Mappings.ProposalsMapping;

public class ProposalMapping : Profile
{
    public ProposalMapping()
    {
        CreateMap<ProjectProposal, ProposalDto>()
            .ForMember(dest => dest.ProjectTitle,
                opt => opt.MapFrom(src => src.Project.Title))

            .ForMember(dest => dest.ApplicantType,
                opt => opt.MapFrom(src => src.ApplicantType.ToString()))

            .ForMember(dest => dest.TeamName,
                opt => opt.MapFrom(src => src.Team != null ? src.Team.Name : null))

            .ForMember(dest => dest.ApplicantName,
                opt => opt.MapFrom(src =>
                    src.ApplicantType == ApplicantType.Team
                        ? (src.Team != null ? src.Team.Name : null)
                        : (src.User != null ? src.User.FristName + " " + src.User.LastName : null)))

            .ForMember(dest => dest.ApplicantAvatarUrl,
                opt => opt.MapFrom(src =>
                    src.ApplicantType == ApplicantType.Team
                        ? (src.Team != null ? src.Team.Logo : null)
                        : (src.User != null ? (src.User.ClientProfile != null ? src.User.ClientProfile.ProfileImage : (src.User.DeveloperProfile != null ? src.User.DeveloperProfile.ProfileImage : null)) : null)))

            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()))

            .ForMember(dest => dest.ChatRoomId,
                opt => opt.MapFrom(src => src.ChatRoom != null ? (Guid?)src.ChatRoom.Id : null))

            .ForMember(dest => dest.AttachmentUrls,
                opt => opt.MapFrom(src => src.ProposalAttachments.Select(a => a.FileUrl)))

            .ForMember(dest => dest.Skills,
                opt => opt.MapFrom(src =>
                    src.ApplicantType == ApplicantType.Team
                        ? (src.Team != null
                            ? src.Team.TeamSkills.Select(ts => ts.Skill.Name)
                            : Enumerable.Empty<string>())
                        : (src.User != null && src.User.DeveloperProfile != null
                            ? src.User.DeveloperProfile.UserSkills.Select(us => us.Skill.Name)
                            : Enumerable.Empty<string>())))

            .ForMember(dest => dest.Specialties,
                opt => opt.MapFrom(src =>
                    src.ApplicantType == ApplicantType.Team
                        ? (src.Team != null
                            ? src.Team.TeamSpecialties.Select(ts => ts.Specialty.NameEn)
                            : Enumerable.Empty<string>())
                        : (src.User != null && src.User.DeveloperProfile != null
                            ? src.User.DeveloperProfile.UserSpecialties.Select(us => us.Specialty.NameEn)
                            : Enumerable.Empty<string>())));
    }
}
