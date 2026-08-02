
namespace FreeGency.Application.Features.Milestones.Commands;

public partial class MilestoneService : IMilestoneService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMilestoneRepository _milestoneRepo;
    private readonly IProjectRepository _projectRepo;
    private readonly ITeamMemberRepository _teamMemberRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public MilestoneService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _milestoneRepo = _unitOfWork.Repository<IMilestoneRepository, Milestone>();
        _projectRepo = _unitOfWork.Repository<IProjectRepository, Project>();
        _teamMemberRepo = _unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
    }
}