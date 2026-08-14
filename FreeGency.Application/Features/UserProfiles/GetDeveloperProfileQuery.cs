namespace FreeGency.Application.Features.UserProfiles
{
    public partial class UserProfile : IUserProfile
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClientProfileRepository _clientProfileRepo;
        private readonly IDeveloperProfileRepository _developerProfileRepo;

        public UserProfile(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _clientProfileRepo = _unitOfWork.Repository<IClientProfileRepository, ClientProfile>();
            _developerProfileRepo = _unitOfWork.Repository<IDeveloperProfileRepository, DeveloperProfile>();
        }

        public async Task<ApiResponse<DeveloperProfileDto>> GetDeveloperProfileAsync(Guid id, CancellationToken ct = default)
        {
            var developerProfile = await _developerProfileRepo
                .GetByUserIdWithSkillsAndInterestsAsync(id, ct);

            if (developerProfile == null)
                return ApiResponse.Failure<DeveloperProfileDto>(
                    AppError.NotFound(nameof(DeveloperProfile), id));

            return ApiResponse.Success(
                _mapper.Map<DeveloperProfileDto>(developerProfile));
        }
    }
}
