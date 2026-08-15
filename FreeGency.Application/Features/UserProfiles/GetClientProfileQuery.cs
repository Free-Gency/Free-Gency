namespace FreeGency.Application.Features.UserProfiles
{
    public partial class UserProfile : IUserProfile
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClientProfileRepository _clientProfileRepo;

        public UserProfile(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _clientProfileRepo = _unitOfWork.Repository<IClientProfileRepository, ClientProfile>();
        }


        public async Task<ApiResponse<ClientProfileDto>> GetClientProfileAsync(Guid id, CancellationToken ct = default)
        {
            var clientProfile = await _clientProfileRepo
               .GetByUserIdWithSkillsAndInterestsAsync(id, ct);

            if (clientProfile == null)
                return ApiResponse.Failure<ClientProfileDto>(
                    AppError.NotFound(nameof(ClientProfile), id));

            return ApiResponse.Success(
                _mapper.Map<ClientProfileDto>(clientProfile));
        }
    }
}