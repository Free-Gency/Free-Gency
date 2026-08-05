namespace FreeGency.Application.Features.Portfolio.Commands
{
    // Queries
    public partial class PortfolioService
    {
        public async Task<ApiResponse<IEnumerable<PortfolioProjectDto>>> GetMineAsync(
            CancellationToken ct = default)
        {
            var repo = _unitOfWork.Repository<IPortfolioRepository, PortfolioProject>();

            var portfolios = await repo.Query()
                .Where(x => x.OwnerUserId == _currentUser.UserId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);

            var dto = _mapper.Map<IEnumerable<PortfolioProjectDto>>(portfolios);

            return ApiResponse.Success(dto);
        }

        public async Task<ApiResponse<IEnumerable<PortfolioProjectDto>>> GetDeveloperPortfolioAsync(
            Guid developerId,
            CancellationToken ct = default)
        {
            var repo = _unitOfWork.Repository<IPortfolioRepository, PortfolioProject>();

            var portfolios = await repo.GetDeveloperPortfolioAsync(developerId, ct);

            var dto = _mapper.Map<IEnumerable<PortfolioProjectDto>>(portfolios);

            return ApiResponse.Success(dto);
        }

        public async Task<ApiResponse<PaginatedResult<PortfolioProjectDto>>> GetInspirationAsync(
            FilterInspirationRequestDto request,
            CancellationToken ct = default)
        {
            var repo = _unitOfWork.Repository<IPortfolioRepository, PortfolioProject>();

            var page = await PaginatedResult<PortfolioProject>.CreateAsync(
                repo.GetInspirationQuery(request.CategoryId, request.Search),
                request.PageNumber,
                request.PageSize,
                ct);

            var mapped = PaginatedResult<PortfolioProjectDto>.FromList(
                _mapper.Map<List<PortfolioProjectDto>>(page.Items),
                page.PageNumber,
                page.PageSize,
                page.TotalCount);

            return ApiResponse.Success(mapped);
        }

        public async Task<ApiResponse> RecordViewAsync(
            Guid portfolioProjectId,
            CancellationToken ct = default)
        {
            var repo = _unitOfWork.Repository<IPortfolioRepository, PortfolioProject>();

            var portfolio = await repo.GetDetailsAsync(portfolioProjectId, ct);
            if (portfolio is null || portfolio.Visibility != Visibility.Public)
                return ApiResponse.Failure(
                    AppError.NotFound(nameof(PortfolioProject), portfolioProjectId));

            await repo.RecordViewAsync(_currentUser.UserId, portfolioProjectId, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse.Success("View recorded.");
        }

        public async Task<ApiResponse<IEnumerable<RecentlyViewedPortfolioDto>>> GetRecentlyViewedAsync(
            int take = 5,
            CancellationToken ct = default)
        {
            var repo = _unitOfWork.Repository<IPortfolioRepository, PortfolioProject>();
            var rows = await repo.GetRecentlyViewedByUserAsync(_currentUser.UserId, take, ct);

            var items = rows.Select(row =>
            {
                var p = row.Project;
                return new RecentlyViewedPortfolioDto(
                    p.Id,
                    p.Title,
                    p.Category?.NameEn ?? p.Category?.Name,
                    p.OwnerTeam?.Name
                        ?? (p.OwnerUser is null
                            ? null
                            : $"{p.OwnerUser.FristName} {p.OwnerUser.LastName}".Trim()),
                    p.ImageCover,
                    row.ViewedAt);
            });

            return ApiResponse.Success(items);
        }

        public async Task<ApiResponse<PortfolioProjectDetailsDto>> GetDetailsAsync(
            Guid id,
            CancellationToken ct = default)
        {
            var repo = _unitOfWork.Repository<IPortfolioRepository, PortfolioProject>();

            var portfolio = await repo.GetDetailsAsync(id, ct);

            if (portfolio is null)
                return ApiResponse.Failure<PortfolioProjectDetailsDto>(
                    AppError.NotFound(nameof(PortfolioProject), id));

            return ApiResponse.Success(
                _mapper.Map<PortfolioProjectDetailsDto>(portfolio));
        }

        public async Task<ApiResponse<PortfolioProjectDetailsDto>> GetPublicDetailsAsync(
            Guid id,
            CancellationToken ct = default)
        {
            var repo = _unitOfWork.Repository<IPortfolioRepository, PortfolioProject>();

            var portfolio = await repo.GetPublicDetailsAsync(id, ct);

            if (portfolio is null)
                return ApiResponse.Failure<PortfolioProjectDetailsDto>(
                    AppError.NotFound(nameof(PortfolioProject), id));

            var dto = _mapper.Map<PortfolioProjectDetailsDto>(portfolio);
            var creator = BuildCreator(portfolio);
            var reviews = await LoadReviewsAsync(portfolio, ct);
            var canEdit = await CanEditPortfolioAsync(portfolio, ct);

            return ApiResponse.Success(new PortfolioProjectDetailsDto
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
                Budget = dto.Budget,
                ImageCover = dto.ImageCover,
                ProjectUrl = dto.ProjectUrl,
                PrototypeUrl = dto.PrototypeUrl,
                CompletionDate = dto.CompletionDate,
                UpdatedAt = dto.UpdatedAt,
                Visibility = dto.Visibility,
                CategoryName = dto.CategoryName,
                OwnerName = creator?.DisplayName ?? dto.OwnerName,
                OwnerType = creator?.Kind ?? portfolio.OwnerType.ToString(),
                OwnerUserId = portfolio.OwnerUserId,
                OwnerTeamId = portfolio.OwnerTeamId,
                Challenge = dto.Challenge,
                Solution = dto.Solution,
                DurationLabel = dto.DurationLabel,
                Industry = dto.Industry,
                TeamLeads = dto.TeamLeads,
                TestimonialQuote = dto.TestimonialQuote,
                TestimonialAuthorName = dto.TestimonialAuthorName,
                TestimonialAuthorTitle = dto.TestimonialAuthorTitle,
                TestimonialAuthorAvatarUrl = dto.TestimonialAuthorAvatarUrl,
                CanEdit = canEdit,
                Creator = creator,
                OwnerReviews = reviews,
                Images = dto.Images,
                Skills = dto.Skills,
                RoadmapSteps = dto.RoadmapSteps,
                Metrics = dto.Metrics,
            });
        }

        public async Task<ApiResponse<OwnerReviewDto>> AddFeedbackAsync(
            Guid portfolioProjectId,
            CreatePortfolioFeedbackRequestDto request,
            CancellationToken ct = default)
        {
            var repo = _unitOfWork.Repository<IPortfolioRepository, PortfolioProject>();

            var portfolio = await repo.GetPublicDetailsAsync(portfolioProjectId, ct);
            if (portfolio is null)
                return ApiResponse.Failure<OwnerReviewDto>(
                    AppError.NotFound(nameof(PortfolioProject), portfolioProjectId));

            if (portfolio.OwnerUserId == _currentUser.UserId)
                return ApiResponse.Failure<OwnerReviewDto>(
                    AppError.Forbidden("You cannot review your own portfolio."));

            if (portfolio.OwnerTeamId is Guid teamId)
            {
                var memberRepo = _unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
                if (await memberRepo.IsMemberAsync(teamId, _currentUser.UserId, ct))
                    return ApiResponse.Failure<OwnerReviewDto>(
                        AppError.Forbidden("You cannot review a portfolio from your own team."));
            }

            if (request.Rating is < 1 or > 5)
                return ApiResponse.Failure<OwnerReviewDto>(
                    AppError.Validation("Rating must be between 1 and 5."));

            var comment = string.IsNullOrWhiteSpace(request.Comment)
                ? null
                : request.Comment.Trim();

            if (comment is { Length: > 500 })
                return ApiResponse.Failure<OwnerReviewDto>(
                    AppError.Validation("Comment must be 500 characters or fewer."));

            if (await repo.HasFeedbackAsync(portfolioProjectId, _currentUser.UserId, ct))
                return ApiResponse.Failure<OwnerReviewDto>(
                    AppError.Conflict("You already reviewed this portfolio."));

            var now = DateTime.UtcNow;
            var feedback = new PortfolioFeedback
            {
                Id = Guid.NewGuid(),
                PortfolioProjectId = portfolioProjectId,
                ReviewerUserId = _currentUser.UserId,
                Rating = request.Rating,
                Comment = comment,
                CreatedAt = now,
                CreatedBy = _currentUser.UserId.ToString(),
            };

            await repo.AddFeedbackAsync(feedback, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var saved = (await repo.GetFeedbackAsync(portfolioProjectId, 50, ct))
                .FirstOrDefault(x => x.Id == feedback.Id);

            return ApiResponse.Success(MapFeedback(saved ?? feedback));
        }

        private async Task<bool> CanEditPortfolioAsync(PortfolioProject portfolio, CancellationToken ct)
        {
            var userId = _currentUser.UserId;
            if (userId == Guid.Empty)
                return false;

            if (portfolio.OwnerUserId == userId)
                return true;

            if (Guid.TryParse(portfolio.CreatedBy, out var creatorId) && creatorId == userId)
                return true;

            if (portfolio.OwnerTeamId is Guid teamId)
                return await CanManageTeamAsync(teamId, ct);

            return false;
        }

        private static PortfolioCreatorDto? BuildCreator(PortfolioProject portfolio)
        {
            if (portfolio.OwnerType == owner.Team && portfolio.OwnerTeam is { } team)
            {
                var specialties = team.TeamSpecialties?
                    .Select(x => x.Specialty?.NameEn)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Cast<string>()
                    .Distinct()
                    .Take(8)
                    .ToList() ?? [];

                var skills = team.TeamSkills?
                    .Select(x => x.Skill?.Name)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Cast<string>()
                    .Distinct()
                    .Take(12)
                    .ToList() ?? [];

                return new PortfolioCreatorDto
                {
                    Kind = "Team",
                    Id = team.Id,
                    DisplayName = team.Name,
                    AvatarUrl = team.Logo,
                    Bio = team.AboutUs,
                    Headline = specialties.FirstOrDefault() ?? "Creative team",
                    AverageRating = team.AverageRating,
                    RatingCount = team.RatingCount,
                    MembersCount = team.TeamMembers?.Count ?? 0,
                    Specialties = specialties,
                    Skills = skills,
                };
            }

            if (portfolio.OwnerUser is { } user)
            {
                var profile = user.DeveloperProfile;
                var specialties = profile?.UserSpecialties?
                    .Select(x => x.Specialty?.NameEn)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Cast<string>()
                    .Distinct()
                    .Take(8)
                    .ToList() ?? [];

                var skills = profile?.UserSkills?
                    .Select(x => x.Skill?.Name)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Cast<string>()
                    .Distinct()
                    .Take(12)
                    .ToList() ?? [];

                var displayName = $"{user.FristName} {user.LastName}".Trim();

                return new PortfolioCreatorDto
                {
                    Kind = "User",
                    Id = user.Id,
                    DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Freelancer" : displayName,
                    AvatarUrl = profile?.ProfileImage,
                    Bio = profile?.Bio,
                    Country = user.Country,
                    Headline = specialties.FirstOrDefault() ?? "Freelancer",
                    AverageRating = profile?.AverageRating ?? 0,
                    RatingCount = profile?.RatingCount ?? 0,
                    MembersCount = 0,
                    Specialties = specialties,
                    Skills = skills,
                };
            }

            return null;
        }

        private async Task<IReadOnlyList<OwnerReviewDto>> LoadReviewsAsync(
            PortfolioProject portfolio,
            CancellationToken ct)
        {
            var repo = _unitOfWork.Repository<IPortfolioRepository, PortfolioProject>();
            var feedback = await repo.GetFeedbackAsync(portfolio.Id, 30, ct);
            return feedback.Select(MapFeedback).ToList();
        }

        private static OwnerReviewDto MapFeedback(PortfolioFeedback feedback)
        {
            var user = feedback.ReviewerUser;
            var name = user is null
                ? string.Empty
                : $"{user.FristName} {user.LastName}".Trim();

            if (string.IsNullOrWhiteSpace(name))
                name = user?.UserName?.Trim() ?? "Community member";

            return new OwnerReviewDto
            {
                Id = feedback.Id,
                Rating = feedback.Rating,
                Comment = feedback.Comment,
                CreatedAt = feedback.CreatedAt,
                ReviewerUserId = feedback.ReviewerUserId,
                ReviewerName = name,
                ReviewerAvatar = user?.DeveloperProfile?.ProfileImage
                    ?? user?.ClientProfile?.ProfileImage,
            };
        }

        public async Task<ApiResponse<IEnumerable<PortfolioProjectDto>>> GetTeamPortfolioAsync(
           Guid teamId,
           CancellationToken ct = default)
        {
            var portfolio = await _portfolioRepo
                .GetTeamPortfolioAsync(teamId, ct);

            return ApiResponse.Success(
                _mapper.Map<IEnumerable<PortfolioProjectDto>>(portfolio));
        }

    }
}
