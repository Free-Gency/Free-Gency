namespace FreeGency.Application.Features.Projects.Commands
{
    // Commands
    public partial class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly ISpecialtyRepository _specialtyRepo;
        private readonly ISkillRepository _skillRepo;
        private readonly ITeamMemberRepository _teamMemberRepo;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProjectService(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _projectRepo = _unitOfWork.Repository<IProjectRepository, Project>();
            _specialtyRepo = _unitOfWork.Repository<ISpecialtyRepository, Specialty>();
            _skillRepo = _unitOfWork.Repository<ISkillRepository, Skill>();
            _categoryRepo = _unitOfWork.Repository<ICategoryRepository, Category>();
            _teamMemberRepo = _unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
        }


        public async Task<ApiResponse<Guid>> CreateAsync(CreateProjectRequestDto request, CancellationToken ct = default)
        {
            if (!await _categoryRepo.ExistsAsync(request.CategoryId))
                return ApiResponse.Failure<Guid>(AppError.NotFound(nameof(Category), request.CategoryId));

            var allowedSpecialties = await _specialtyRepo.GetByCategoryIdAsync(request.CategoryId, ct);

            var project = new Project
            {
                Title = request.Title,
                Description = request.Description,
                ClientId = _currentUser.UserId,
                CategoryId = request.CategoryId,
                IsFixedPrice = request.IsFixedPrice,
                BudgetMin = request.BudgetMin,
                BudgetMax = request.BudgetMax,
                Currency = request.Currency,
                EstimatedDurationDays = request.EstimatedDurationDays,
            };

            foreach (var specialtyId in request.SpecialtyIds.Distinct())
            {
                if (allowedSpecialties.Any(s => s.Id == specialtyId))
                {
                    project.ProjectSpecialties.Add(new ProjectSpecialty
                    {
                        SpecialtyId = specialtyId
                    });
                }
                else
                {
                    return ApiResponse.Failure<Guid>(AppError.SpecialtyDoesNotBelongToCategory(specialtyId, request.CategoryId));
                }
            }

            var allowedSkills = new List<Skill>();

            foreach (var specialty in allowedSpecialties)
            {
                allowedSkills.AddRange(await _skillRepo.GetBySpecialtyIdAsync(specialty.Id));
            }

            foreach (var skillId in request.SkillIds.Distinct())
            {
                if (allowedSkills.Any(s => s.Id == skillId))
                {
                    project.ProjectSkills.Add(new ProjectSkill
                    {
                        SkillId = skillId
                    });
                }
                else
                {
                    return ApiResponse.Failure<Guid>(AppError.SkillDoesNotBelongToSpecialty(skillId));
                }
            }

            await _projectRepo.AddAsync(project, ct);

            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse.Success(project.Id, "The Project has been successfully created.");
        }

        public async Task<ApiResponse> SaveAsync(Guid id, CancellationToken ct = default)
        {
            if (!await _projectRepo.ExistsAsync(id, ct))
                return ApiResponse.Failure(AppError.NotFound(nameof(Project), id));

            await _projectRepo.SaveProjectAsync(id, _currentUser.UserId, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse.Success("Project saved successfully.");
        }

        public async Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var project = await _projectRepo.GetByIdAsync(id, ct);
            if (project == null)
                return ApiResponse.Failure(AppError.NotFound(nameof(Project), id));

            _projectRepo.Delete(project);
            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse.Success("Project deleted Successfully.");
        }

        public async Task<ApiResponse> UnSaveAsync(Guid id, CancellationToken ct = default)
        {
            if (!await _projectRepo.ExistsAsync(id, ct))
                return ApiResponse.Failure(AppError.NotFound(nameof(Project), id));

            await _projectRepo.UnsaveProjectAsync(id, _currentUser.UserId, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse.Success("Project unsaved successfully.");
        }

        public async Task<ApiResponse> EditAsync(UpdateProjectRequestDto request, CancellationToken ct = default)
        {
            var project = await _projectRepo.GetByIdAsync(request.Id, ct);

            if (project is null)
                return ApiResponse.Failure(AppError.NotFound(nameof(Project), request.Id));

            if (project.ClientId != _currentUser.UserId)
                return ApiResponse.Failure(AppError.Forbidden("You do not own this project."));

            if (project.Status != ProjectStatus.Draft && project.Status != ProjectStatus.Open)
                return ApiResponse.Failure(AppError.Validation("Only draft or open projects can be edited."));

            if (request.Title is not null)
                project.Title = request.Title;

            if (request.Description is not null)
                project.Description = request.Description;

            if (request.CategoryId.HasValue)
                project.CategoryId = request.CategoryId.Value;

            if (request.IsFixedPrice.HasValue)
                project.IsFixedPrice = request.IsFixedPrice.Value;

            if (request.BudgetMin.HasValue)
                project.BudgetMin = request.BudgetMin.Value;

            if (request.BudgetMax.HasValue)
                project.BudgetMax = request.BudgetMax.Value;

            if (request.Currency is not null)
                project.Currency = request.Currency;

            if (request.EstimatedDurationDays.HasValue)
                project.EstimatedDurationDays = request.EstimatedDurationDays.Value;

            if (request.SkillIds != null)
            {
                await _projectRepo.ReplaceSkillsAsync(request.Id, request.SkillIds, ct);
            }

            if (request.SpecialtyIds != null)
            {
                await _projectRepo.ReplaceSpecialtiesAsync(request.Id, request.SpecialtyIds, ct);
            }

            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse.Success("Project updated successfully.");
        }

        public async Task<ApiResponse> PublishAsync(Guid id, CancellationToken ct = default)
        {
            var project = await _projectRepo.GetByIdAsync(id, ct);
            if (project == null)
                return ApiResponse.Failure(AppError.NotFound(nameof(Project), id));

            if (project.ClientId != _currentUser.UserId)
                return ApiResponse.Failure(AppError.Forbidden("You do not own this project."));

            project.Status = ProjectStatus.Open;
            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse.Success("Project published successfully.");
        }

        public async Task<ApiResponse> ReplaceSkillsAsync(Guid id, IEnumerable<Guid> skillIds, CancellationToken ct = default)
        {
            if (!await _projectRepo.ExistsAsync(id, ct))
                return ApiResponse.Failure(AppError.NotFound(nameof(Project), id));

            if (skillIds.Count() > 0)
            {
                await _projectRepo.ReplaceSkillsAsync(id, skillIds, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }
            else
            {
                return ApiResponse.Failure(AppError.Validation("Skills not found, please provide at least one skill."));
            }

            return ApiResponse.Success("Project's skills has been replaced successfully.");
        }

    }
}
