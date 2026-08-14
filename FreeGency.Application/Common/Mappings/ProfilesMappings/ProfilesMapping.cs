namespace FreeGency.Application.Common.Mappings.ProfilesMappings
{
    public class ProfilesMapping : Profile
    {
        public ProfilesMapping()
        {
            CreateMap<DeveloperProfile, DeveloperProfileDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                    $"{src.User.FristName} {src.User.LastName}"))

                .ForMember(dest => dest.UserInterests, opt => opt.MapFrom(src =>
                    src.UserInterests.Select(i => i.Category.Name)))

                .ForMember(dest => dest.UserSpecialties, opt => opt.MapFrom(src =>
                    src.UserSpecialties.Select(s => s.Specialty.NameEn)))

                .ForMember(dest => dest.UserSkills, opt => opt.MapFrom(src =>
                    src.UserSkills.Select(us => us.Skill.Name)));



            CreateMap<ClientProfile, ClientProfileDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                    $"{src.User.FristName} {src.User.LastName}"))

                .ForMember(dest => dest.UserInterests, opt => opt.MapFrom(src =>
                    src.UserInterests.Select(i => i.Category.Name)))

                .ForMember(dest => dest.UserSpecialties, opt => opt.MapFrom(src =>
                    src.UserSpecialties.Select(s => s.Specialty.NameEn)))

                .ForMember(dest => dest.UserSkills, opt => opt.MapFrom(src =>
                    src.UserSkills.Select(us => us.Skill.Name)));

        }
    }
}
