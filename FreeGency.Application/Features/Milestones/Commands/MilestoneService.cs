using FreeGency.Application.Common.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace FreeGency.Application.Features.Milestones.Commands;

public partial class MilestoneService : IMilestoneService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMilestoneRepository _milestoneRepo;
    private readonly IProjectRepository _projectRepo;
    private readonly ITeamMemberRepository _teamMemberRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IHubContext<ChatHub> _hub;

    public MilestoneService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IMapper mapper,
        IHubContext<ChatHub> hub)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _hub = hub;
        _milestoneRepo = _unitOfWork.Repository<IMilestoneRepository, Milestone>();
        _projectRepo = _unitOfWork.Repository<IProjectRepository, Project>();
        _teamMemberRepo = _unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
    }
}
