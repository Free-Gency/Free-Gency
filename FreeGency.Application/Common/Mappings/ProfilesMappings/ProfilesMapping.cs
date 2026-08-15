namespace FreeGency.Application.Common.Mappings.ProfilesMappings
{
    public class ProfilesMapping : Profile
    {
        public ProfilesMapping()
        {

            CreateMap<ClientProfile, ClientProfileDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                    $"{src.User.FristName} {src.User.LastName}"))

                .ForMember(dest => dest.UserInterests, opt => opt.MapFrom(src =>
                    src.UserInterests.Select(i => i.Category.Name)))

                .ForMember(dest => dest.UserSpecialties, opt => opt.MapFrom(src =>
                    src.UserSpecialties.Select(s => s.Specialty.NameEn)))

                .ForMember(dest => dest.UserSkills, opt => opt.MapFrom(src =>
                    src.UserSkills.Select(us => us.Skill.Name)))

                .ForMember(dest => dest.IsVerified, opt => opt.MapFrom(src =>
                    src.User.IsVerified))

                .ForMember(dest => dest.Country, opt => opt.MapFrom(src =>
                    src.User.Country))

                .ForMember(dest => dest.OwnedTeams, opt => opt.MapFrom(src =>
                    src.User.OwnedTeams.Select(t => t.Name)))

                .ForMember(dest => dest.PostedProjects, opt => opt.MapFrom(src =>
                    src.User.PostedProjects))

                .ForMember(dest => dest.SocialLinks, opt => opt.MapFrom(src =>
                    src.User.SocialLinks))

                .ForMember(dest => dest.ReviewsReceived, opt => opt.MapFrom(src =>
                    src.User.ReviewsReceived))

                .ForMember(dest => dest.PortfolioProjects, opt => opt.MapFrom(src =>
                    src.User.PortfolioProjects));

        }
    }
}
