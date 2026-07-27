namespace FreeGency.Application.Features.ProjectEvents.Commands;

public partial class ProjectEventService : IProjectEventService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProjectEventRepository _eventRepo;
    private readonly IProjectRepository _projectRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public ProjectEventService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _eventRepo = _unitOfWork.Repository<IProjectEventRepository, ProjectEvent>();
        _projectRepo = _unitOfWork.Repository<IProjectRepository, Project>();
    }
}