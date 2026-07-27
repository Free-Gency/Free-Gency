
namespace FreeGency.Application.Features.Escrow.Commands;

public partial class EscrowService : IEscrowService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEscrowHoldRepository _escrowRepo;
    private readonly IProjectRepository _projectRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public EscrowService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _escrowRepo = _unitOfWork.Repository<IEscrowHoldRepository, EscrowHold>();
        _projectRepo = _unitOfWork.Repository<IProjectRepository, Project>();
    }
}