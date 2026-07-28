
using FreeGency.Application.Features.Escrow.DTOs;

namespace FreeGency.Application.Common.Mappings.EscrowMappings;

public class EscrowMapping : Profile
{
    public EscrowMapping()
    {
        CreateMap<EscrowHold, EscrowDto>()
            .ForMember(dest => dest.FundingStatus, opt => opt.MapFrom(src => src.FundingStatus.ToString()))
            .ForMember(dest => dest.PlanStatus, opt => opt.MapFrom(src => src.planStatus.ToString()));
    }
}
