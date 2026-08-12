namespace FreeGency.Application.Features.Portfolio.Commands
{
    // Commands
    public partial class PortfolioService : IPortfolioService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStorageService _storageService;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        private readonly IPortfolioRepository _portfolioRepo;
        private readonly ITeamRepository _teamRepo;

        public PortfolioService(
            IUnitOfWork unitOfWork,
            IStorageService storageService,
            ICurrentUserService currentUser,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _storageService = storageService;
            _currentUser = currentUser;
            _mapper = mapper;

            _portfolioRepo = unitOfWork.Repository<IPortfolioRepository, PortfolioProject>();
            _teamRepo = unitOfWork.Repository<ITeamRepository, Team>();
        }

        public async Task<ApiResponse<Guid>> CreateAsync(
            CreatePortfolioProjectRequestDto request,
            CancellationToken ct = default)
        {
            await _unitOfWork.BeginTransactionAsync(ct);

            try
            {
                var portfolio = new PortfolioProject
                {
                    OwnerType = request.OwnerType,

                    OwnerUserId = request.OwnerType == owner.User
                        ? _currentUser.UserId
                        : null,

                    OwnerTeamId = request.OwnerType == owner.Team
                        ? request.OwnerTeamId
                        : null,

                    Title = request.Title.Trim(),
                    Description = (request.Description ?? string.Empty).Trim(),
                    Budget = request.Budget,
                    ProjectUrl = Truncate(NullIfWhiteSpace(request.ProjectUrl), 500),
                    PrototypeUrl = Truncate(NullIfWhiteSpace(request.PrototypeUrl), 500),
                    CompletionDate = request.CompletionDate,
                    CategoryId = request.CategoryId,
                    Visibility = request.Visibility,
                    Challenge = request.Challenge,
                    Solution = request.Solution,
                    DurationLabel = Truncate(request.DurationLabel, 100),
                    Industry = Truncate(request.Industry, 120),
                    TeamLeads = Truncate(request.TeamLeads, 2000),
                    TestimonialQuote = request.TestimonialQuote,
                    TestimonialAuthorName = Truncate(request.TestimonialAuthorName, 150),
                    TestimonialAuthorTitle = Truncate(request.TestimonialAuthorTitle, 200),
                    TestimonialAuthorAvatarUrl = Truncate(request.TestimonialAuthorAvatarUrl, 500),
                };

                if (string.IsNullOrWhiteSpace(portfolio.Description))
                    portfolio.Description = portfolio.Title;

                //----------------------------------------------------
                // Team ownership validation
                //----------------------------------------------------

                if (portfolio.OwnerType == owner.Team)
                {
                    if (!portfolio.OwnerTeamId.HasValue)
                        return ApiResponse.Failure<Guid>(
                            AppError.Validation("TeamId is required."));

                    var team = await _teamRepo.GetByIdAsync(
                        portfolio.OwnerTeamId.Value,
                        ct);

                    if (team is null)
                        return ApiResponse.Failure<Guid>(
                            AppError.NotFound(nameof(Team), portfolio.OwnerTeamId));

                    if (team.OwnerUserId != _currentUser.UserId)
                    {
                        return ApiResponse.Failure<Guid>(
                            AppError.Forbidden("You are not allowed to create portfolio for this team."));
                    }
                }

                //----------------------------------------------------
                // Save Portfolio + Skills
                //----------------------------------------------------

                await _portfolioRepo.AddWithSkillsAsync(
                    portfolio,
                    request.SkillIds ?? [],
                    ct);

                await ReplaceCaseStudyCollectionsAsync(
                    portfolio.Id,
                    request.RoadmapSteps,
                    request.Metrics,
                    ct);

                //----------------------------------------------------
                // Images (optional — failure should not hide the root cause)
                //----------------------------------------------------

                if (request.Images is not null &&
                    request.Images.Any())
                {
                    try
                    {
                        var uploaded =
                            await _storageService.UploadManyAsync(
                                request.Images,
                                "portfolio",
                                ct);

                        await _portfolioRepo.AddImagesAsync(
                            portfolio.Id,
                            uploaded.Select(x => x.Url),
                            ct);

                        portfolio.ImageCover =
                            uploaded.First().Url;
                    }
                    catch
                    {
                        // Keep the portfolio even if Cloudinary/storage fails during onboarding.
                    }
                }

                await _unitOfWork.SaveChangesAsync(ct);

                await _unitOfWork.CommitTransactionAsync(ct);

                return ApiResponse.Success(
                    portfolio.Id,
                    "Portfolio created successfully.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                return ApiResponse.Failure<Guid>(
                    AppError.Validation(ex.InnerException?.Message ?? ex.Message));
            }

        }

        public async Task<ApiResponse> UpdateAsync(
            Guid id,
            UpdatePortfolioProjectRequestDto request,
            CancellationToken ct = default)
        {
            var portfolio = await _portfolioRepo.GetDetailsAsync(id, ct);

            if (portfolio is null)
                return ApiResponse.Failure(AppError.NotFound(nameof(PortfolioProject), id));

            if (portfolio.OwnerUserId != _currentUser.UserId)
                return ApiResponse.Failure(AppError.Forbidden());

            // Update editable fields only
            portfolio.Title = request.Title;
            portfolio.Description = request.Description;
            portfolio.ProjectUrl = request.ProjectUrl;
            portfolio.PrototypeUrl = request.PrototypeUrl;
            portfolio.Budget = request.Budget;
            portfolio.CategoryId = request.CategoryId;
            portfolio.CompletionDate = request.CompletionDate;
            portfolio.Visibility = request.Visibility;
            portfolio.Challenge = request.Challenge;
            portfolio.Solution = request.Solution;
            portfolio.DurationLabel = request.DurationLabel;
            portfolio.Industry = request.Industry;
            portfolio.TeamLeads = request.TeamLeads;
            portfolio.TestimonialQuote = request.TestimonialQuote;
            portfolio.TestimonialAuthorName = request.TestimonialAuthorName;
            portfolio.TestimonialAuthorTitle = request.TestimonialAuthorTitle;
            portfolio.TestimonialAuthorAvatarUrl = request.TestimonialAuthorAvatarUrl;

            _portfolioRepo.Update(portfolio);

            if (request.RoadmapSteps is not null || request.Metrics is not null)
            {
                await ReplaceCaseStudyCollectionsAsync(
                    portfolio.Id,
                    request.RoadmapSteps,
                    request.Metrics,
                    ct);
            }

            if (request.SkillIds is not null)
            {
                await _portfolioRepo.ReplaceSkillsAsync(id, request.SkillIds, ct);
            }

            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse.Success("Portfolio project updated successfully.");
        }

        public async Task<ApiResponse> DeleteAsync(
            Guid id,
            CancellationToken ct = default)
        {
            await _unitOfWork.BeginTransactionAsync(ct);

            try
            {
                var portfolio = await GetPortfolioOrThrowAsync(id, ct);

                await EnsureCanEditAsync(portfolio, ct);

                await _portfolioRepo.DeleteImagesAndSkillsAsync(id, ct);
                _portfolioRepo.Delete(portfolio);

                await _unitOfWork.SaveChangesAsync(ct);
                await _unitOfWork.CommitTransactionAsync(ct);

                return ApiResponse.Success("Portfolio deleted successfully.");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                throw;
            }
        }

        public async Task<ApiResponse> ReplaceSkillsAsync(
            Guid portfolioId,
            IEnumerable<Guid> skillIds,
            CancellationToken ct = default)
        {
            var portfolio = await GetPortfolioOrThrowAsync(
                portfolioId,
                ct);

            await EnsureCanEditAsync(
                portfolio,
                ct);

            await _portfolioRepo.ReplaceSkillsAsync(
                portfolioId,
                skillIds,
                ct);

            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse.Success(
                "Skills updated successfully.");
        }

        public async Task<ApiResponse> UploadImagesAsync(
            Guid portfolioId,
            IEnumerable<IFormFile> images,
            CancellationToken ct = default)
        {
            if (images is null || !images.Any())
                return ApiResponse.Failure(
                    AppError.Validation("Please select at least one image."));

            await _unitOfWork.BeginTransactionAsync(ct);

            try
            {
                var portfolio = await GetPortfolioOrThrowAsync(portfolioId, ct);

                await EnsureCanEditAsync(portfolio, ct);

                //---------------------------------------------------
                // Upload to Cloudinary
                //---------------------------------------------------

                var uploadedImages = await _storageService.UploadManyAsync(
                    images,
                    "portfolio",
                    ct);

                //---------------------------------------------------
                // Save database records
                //---------------------------------------------------

                await _portfolioRepo.AddImagesAsync(
                    portfolioId,
                    uploadedImages.Select(x => x.Url),
                    ct);

                //---------------------------------------------------
                // First image becomes cover automatically
                //---------------------------------------------------

                if (string.IsNullOrWhiteSpace(portfolio.ImageCover))
                {
                    portfolio.ImageCover = uploadedImages.First().Url;

                    _portfolioRepo.Update(portfolio);
                }

                await _unitOfWork.SaveChangesAsync(ct);
                await _unitOfWork.CommitTransactionAsync(ct);

                return ApiResponse.Success("Images uploaded successfully.");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                throw;
            }
        }

        public async Task<ApiResponse> DeleteImageAsync(
            Guid portfolioId,
            Guid imageId,
            CancellationToken ct = default)
        {
            await _unitOfWork.BeginTransactionAsync(ct);

            try
            {
                var portfolio = await GetPortfolioOrThrowAsync(
                    portfolioId,
                    ct);

                await EnsureCanEditAsync(
                    portfolio,
                    ct);

                var image = portfolio.PortfolioImages
                    .FirstOrDefault(x => x.Id == imageId);

                if (image is null)
                    return ApiResponse.Failure(
                        AppError.NotFound(nameof(PortfolioImage), imageId));

                //---------------------------------------------------
                // Delete image
                //---------------------------------------------------

                await _portfolioRepo.DeleteImageAsync(
                    imageId,
                    ct);

                //---------------------------------------------------
                // Refresh local collection
                //---------------------------------------------------

                portfolio.PortfolioImages.Remove(image);

                //---------------------------------------------------
                // Update cover image
                //---------------------------------------------------

                if (portfolio.ImageCover == image.ImageUrl)
                {
                    portfolio.ImageCover = portfolio.PortfolioImages
                        .OrderBy(x => x.SortOrder)
                        .Select(x => x.ImageUrl)
                        .FirstOrDefault();

                    _portfolioRepo.Update(portfolio);
                }

                await _unitOfWork.SaveChangesAsync(ct);
                await _unitOfWork.CommitTransactionAsync(ct);

                return ApiResponse.Success("Image deleted successfully.");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                throw;
            }
        }

        public async Task<ApiResponse<Guid>> CreateForTeamAsync(
            Guid teamId,
            CreatePortfolioProjectRequestDto request,
            CancellationToken ct = default)
        {
            if (!await CanManageTeamAsync(teamId, ct))
                return ApiResponse.Failure<Guid>(
                    AppError.Forbidden("Only the team owner or a team leader can publish portfolio projects."));

            var project = _mapper.Map<PortfolioProject>(request);

            project.Id = Guid.NewGuid();
            project.OwnerType = owner.Team;
            project.OwnerTeamId = teamId;
            project.OwnerUserId = null;
            project.CreatedBy = _currentUser.UserId.ToString();
            project.Title = (request.Title ?? string.Empty).Trim();
            project.Description = (request.Description ?? string.Empty).Trim();
            if (project.TeamLeads is { Length: > 2000 })
                project.TeamLeads = project.TeamLeads[..2000];

            await _portfolioRepo.AddWithSkillsAsync(
                project,
                request.SkillIds,
                ct);

            await ReplaceCaseStudyCollectionsAsync(
                project.Id,
                request.RoadmapSteps,
                request.Metrics,
                ct);

            await _unitOfWork.SaveChangesAsync(ct);

            if (request.Images?.Any() == true)
                await UploadTeamImagesAsync(teamId, project.Id, request.Images, ct);

            return ApiResponse.Success(project.Id, "Portfolio has been created successfully.");
        }

        public async Task<ApiResponse> UpdateForTeamAsync(
            Guid teamId,
            Guid id,
            UpdatePortfolioProjectRequestDto request,
            CancellationToken ct = default)
        {
            if (!await CanManageTeamAsync(teamId, ct))
                return ApiResponse.Failure(AppError.Unauthorized());

            var portfolio = await _portfolioRepo.GetDetailsAsync(id, ct);

            if (portfolio is null)
                return ApiResponse.Failure(AppError.NotFound(nameof(PortfolioProject), id));

            if (portfolio.OwnerTeamId != teamId)
                return ApiResponse.Failure(AppError.Forbidden());

            _mapper.Map(request, portfolio);

            _portfolioRepo.Update(portfolio);

            if (request.RoadmapSteps is not null || request.Metrics is not null)
            {
                await ReplaceCaseStudyCollectionsAsync(
                    portfolio.Id,
                    request.RoadmapSteps,
                    request.Metrics,
                    ct);
            }

            if (request.SkillIds is not null)
            {
                await _portfolioRepo.ReplaceSkillsAsync(id, request.SkillIds, ct);
            }

            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse.Success("Portfolio has been updated successfully.");
        }

        public async Task<ApiResponse> DeleteForTeamAsync(
            Guid teamId,
            Guid id,
            CancellationToken ct = default)
        {
            if (!await CanManageTeamAsync(teamId, ct))
                return ApiResponse.Failure(AppError.Unauthorized());

            var portfolio = await _portfolioRepo.GetDetailsAsync(id, ct);

            if (portfolio is null)
                return ApiResponse.Failure(AppError.NotFound(nameof(PortfolioProject), id));

            if (portfolio.OwnerTeamId != teamId)
                return ApiResponse.Failure(AppError.Forbidden());

            _portfolioRepo.Delete(portfolio);

            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse.Success("Portfolio has been deleted successfully.");
        }

        public async Task<ApiResponse> ReplaceTeamSkillsAsync(
            Guid teamId,
            Guid portfolioProjectId,
            IEnumerable<Guid> skillIds,
            CancellationToken ct = default)
        {
            if (!await CanManageTeamAsync(teamId, ct))
                return ApiResponse.Failure(AppError.Unauthorized());

            var portfolio = await _portfolioRepo
                .GetDetailsAsync(portfolioProjectId, ct);

            if (portfolio is null)
                return ApiResponse.Failure(AppError.NotFound(nameof(PortfolioProject), portfolioProjectId));

            if (portfolio.OwnerTeamId != teamId)
                return ApiResponse.Failure(AppError.Forbidden());

            await _portfolioRepo.ReplaceSkillsAsync(
                portfolioProjectId,
                skillIds,
                ct);

            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse.Success("skills have been replaced successfully.");
        }

        public async Task<ApiResponse> UploadTeamImagesAsync(
            Guid teamId,
            Guid portfolioProjectId,
            IEnumerable<IFormFile> images,
            CancellationToken ct = default)
        {
            if (!await CanManageTeamAsync(teamId, ct))
                return ApiResponse.Failure(AppError.Unauthorized());

            var portfolio = await _portfolioRepo
                .GetDetailsAsync(portfolioProjectId, ct);

            if (portfolio is null)
                return ApiResponse.Failure(AppError.NotFound(nameof(PortfolioProject), portfolioProjectId));

            if (portfolio.OwnerTeamId != teamId)
                return ApiResponse.Failure(AppError.Forbidden());

            var uploaded = await _storageService.UploadManyAsync(
                images,
                $"portfolio/team/{teamId}",
                ct);

            await _portfolioRepo.AddImagesAsync(
                portfolioProjectId,
                uploaded.Select(x => x.Url),
                ct);

            if (string.IsNullOrWhiteSpace(portfolio.ImageCover))
            {
                portfolio.ImageCover = uploaded.First().Url;
                _portfolioRepo.Update(portfolio);
            }

            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse.Success("Team images has been uploaded successfully.");
        }

        public async Task<ApiResponse> DeleteTeamImageAsync(
            Guid teamId,
            Guid imageId,
            CancellationToken ct = default)
        {
            if (!await CanManageTeamAsync(teamId, ct))
                return ApiResponse.Failure(AppError.Unauthorized());

            var image = await _portfolioRepo.GetImageByIdAsync(imageId, ct);

            if (image is null)
                return ApiResponse.Failure(AppError.NotFound(nameof(PortfolioImage), imageId));

            var portfolio = await _portfolioRepo
                .GetDetailsAsync(image.PortfolioProjectId, ct);

            if (portfolio is null)
                return ApiResponse.Failure(AppError.NotFound(nameof(PortfolioProject), image.PortfolioProjectId));

            if (portfolio.OwnerTeamId != teamId)
                return ApiResponse.Failure(AppError.Forbidden());

            await _portfolioRepo.DeleteImageAsync(imageId, ct);

            await _unitOfWork.SaveChangesAsync(ct);

            var remaining = portfolio.PortfolioImages
                .Where(x => x.Id != imageId)
                .OrderBy(x => x.SortOrder)
                .ToList();

            portfolio.ImageCover = remaining.FirstOrDefault()?.ImageUrl;

            _portfolioRepo.Update(portfolio);

            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse.Success("Team image has been deleted successfully.");
        }


        #region Helpers
        private async Task<PortfolioProject> GetPortfolioOrThrowAsync(
            Guid portfolioId,
            CancellationToken ct)
        {
            var portfolio =
                await _portfolioRepo.GetDetailsAsync(
                    portfolioId,
                    ct);

            if (portfolio is null)
                throw new KeyNotFoundException("Portfolio not found.");

            return portfolio;
        }

        private async Task EnsureCanEditAsync(
            PortfolioProject portfolio,
            CancellationToken ct)
        {
            if (portfolio.OwnerType == owner.User)
            {
                if (portfolio.OwnerUserId != _currentUser.UserId)
                    throw new UnauthorizedAccessException();
            }
            else
            {
                var team =
                    await _teamRepo.GetByIdAsync(
                        portfolio.OwnerTeamId!.Value,
                        ct);

                if (team is null)
                    throw new KeyNotFoundException();

                if (team.OwnerUserId != _currentUser.UserId)
                    throw new UnauthorizedAccessException();

                // TODO:
                // Later support Team Admin here
            }
        }

        private async Task<bool> CanManageTeamAsync(Guid teamId, CancellationToken ct)
        {
            var team = await _unitOfWork
                .Repository<ITeamRepository, Team>()
                .GetByIdAsync(teamId, ct);

            if (team is null)
                return false;

            // Owner
            if (team.OwnerUserId == _currentUser.UserId)
                return true;

            // Team Admin
            return await _unitOfWork
                .Repository<ITeamMemberRepository, TeamMember>()
                .IsLeaderAsync(teamId, _currentUser.UserId, ct);
        }

        private async Task ReplaceCaseStudyCollectionsAsync(
            Guid portfolioProjectId,
            IEnumerable<PortfolioRoadmapStepDto>? steps,
            IEnumerable<PortfolioMetricDto>? metrics,
            CancellationToken ct)
        {
            if (steps is not null)
            {
                var entities = steps
                    .Where(s => !string.IsNullOrWhiteSpace(s.Title))
                    .Select((s, index) => new PortfolioRoadmapStep
                    {
                        Id = Guid.NewGuid(),
                        PortfolioProjectId = portfolioProjectId,
                        Title = Truncate(s.Title.Trim(), 200)!,
                        SortOrder = s.SortOrder != 0 ? s.SortOrder : index,
                        IsDone = s.IsDone,
                    })
                    .ToList();

                await _portfolioRepo.ReplaceRoadmapStepsAsync(portfolioProjectId, entities, ct);
            }

            if (metrics is not null)
            {
                var entities = metrics
                    .Where(m => !string.IsNullOrWhiteSpace(m.Value) && !string.IsNullOrWhiteSpace(m.Label))
                    .Select((m, index) => new PortfolioMetric
                    {
                        Id = Guid.NewGuid(),
                        PortfolioProjectId = portfolioProjectId,
                        Value = Truncate(m.Value.Trim(), 50)!,
                        Label = Truncate(m.Label.Trim(), 120)!,
                        SortOrder = m.SortOrder != 0 ? m.SortOrder : index,
                    })
                    .ToList();

                await _portfolioRepo.ReplaceMetricsAsync(portfolioProjectId, entities, ct);
            }
        }

        private static string? NullIfWhiteSpace(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string? Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var trimmed = value.Trim();
            return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
        }

        #endregion
    }
}
