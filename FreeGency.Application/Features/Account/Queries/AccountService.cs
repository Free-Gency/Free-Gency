using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Common.Models;
using FreeGency.Application.Features.Account.Dtos;
using FreeGency.Application.Features.Account.Mapping;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Domain.Interfaces.Repositories.Reviews;
using FreeGency.Domain.Specifications;
using FreeGency.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.Account.Queries
{
    public partial class AccountService (
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IStorageService storageService,
        UserManager<User> userManager,
        IContentModerationService contentModerationService): IAccountService
    {

        private readonly IClientProfileRepository _profileRepository = unitOfWork.Repository<IClientProfileRepository, ClientProfile>();
        private readonly IDeveloperProfileRepository _developerProfileRepository = unitOfWork.Repository<IDeveloperProfileRepository, DeveloperProfile>();
        private readonly IUserRepository _userRepository = unitOfWork.Repository<IUserRepository, User>();


        public async Task<Result<ClientAccountResponseDto>> GetClientProfile()
        {
            var userId = currentUserService.UserId;
            if (userId==Guid.Empty) return Result.Failure<ClientAccountResponseDto>(UserErrors.UserNotFound);
            var spec = new ClientAccountSpecifiaction(userId);
            var repo = unitOfWork.Repository<IClientProfileRepository, ClientProfile>();
            var clientAccount = await repo.GetEntityWithSpec(spec);
            if(clientAccount==null)return Result.Failure<ClientAccountResponseDto>(UserErrors.UserNotFound);
            var response = clientAccount.ToDto();
            response.ProfileImage = ResolveProfileImageUrl(clientAccount.ProfileImage);

            var projectRepo = unitOfWork.Repository<IProjectRepository, Project>();
            var clientProjects = projectRepo.GetProjectsQuery()
                .Where(p => p.ClientId == userId);

            response.ProjectsPostedCount = await clientProjects.CountAsync();
            response.ProjectsCompletedCount = await clientProjects
                .CountAsync(p => p.Status == ProjectStatus.Completed);

            return Result.Success(response);
        }

        public async Task<Result<List<ProfileInterestDto>>> GetClientInterests()
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
                return Result.Failure<List<ProfileInterestDto>>(UserErrors.UserNotFound);

            var spec = ClientAccountSpecifiaction.ForInterestCatalog(userId);
            var repo = unitOfWork.Repository<IClientProfileRepository, ClientProfile>();
            var clientAccount = await repo.GetEntityWithSpec(spec);
            if (clientAccount is null)
                return Result.Failure<List<ProfileInterestDto>>(UserErrors.UserNotFound);

            return Result.Success(clientAccount.ToInterestCatalog());
        }

        public async Task<Result<DeveloperAccountResponseDto>> GetDeveloperProfile()
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty) return Result.Failure<DeveloperAccountResponseDto>(UserErrors.UserNotFound);
            return await GetDeveloperPublicProfileAsync(userId);
        }

        public async Task<Result<DeveloperAccountResponseDto>> GetDeveloperPublicProfileAsync(Guid userId)
        {
            if (userId == Guid.Empty) return Result.Failure<DeveloperAccountResponseDto>(UserErrors.UserNotFound);
            var spec = new DeveloperAccountSpecification(userId, true);
            var developerProfile = await _developerProfileRepository.GetEntityWithSpec(spec);
            if (developerProfile is null)
                return Result.Failure<DeveloperAccountResponseDto>(UserErrors.UserNotFound);

            var response = developerProfile.ToDto();
            response.ProfileImage = ResolveProfileImageUrl(developerProfile.ProfileImage);

            var portfolioRepo = unitOfWork.Repository<IPortfolioRepository, PortfolioProject>();
            response.TotalJobs = await portfolioRepo.Query()
                .CountAsync(p =>
                    p.OwnerUserId == userId
                    && (p.CompletionDate != null || p.Visibility == Visibility.Public));

            return Result.Success(response);
        }

        public async Task<Result<PaginatedResult<DeveloperBrowseDto>>> BrowseDevelopersAsync(
            FilterDevelopersRequestDto filter,
            CancellationToken ct = default)
        {
            var query = _developerProfileRepository.Query()
                .AsNoTracking()
                .Include(d => d.User)
                .Include(d => d.UserInterests).ThenInclude(i => i.Category)
                .Include(d => d.UserSpecialties).ThenInclude(s => s.Specialty)
                .Include(d => d.UserSkills).ThenInclude(s => s.Skill)
                .Where(d => !d.IsDeleted && d.User != null && !d.User.IsDeleted);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var term = filter.Search.Trim().ToLower();
                query = query.Where(d =>
                    (d.User.FristName + " " + d.User.LastName).ToLower().Contains(term)
                    || (d.Bio != null && d.Bio.ToLower().Contains(term)));
            }

            if (filter.CategoryId.HasValue)
            {
                var categoryId = filter.CategoryId.Value;
                query = query.Where(d => d.UserInterests.Any(i => i.CategoryId == categoryId));
            }

            if (filter.SpecialtyId.HasValue)
            {
                var specialtyId = filter.SpecialtyId.Value;
                query = query.Where(d => d.UserSpecialties.Any(s => s.SpecialtyId == specialtyId));
            }

            if (filter.SkillId.HasValue)
            {
                var skillId = filter.SkillId.Value;
                query = query.Where(d => d.UserSkills.Any(s => s.SkillId == skillId));
            }

            var total = await query.CountAsync(ct);
            var page = await query
                .OrderByDescending(d => d.AverageRating)
                .ThenByDescending(d => d.RatingCount)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(ct);

            var items = page.Select(d =>
            {
                var title = d.UserSpecialties
                    .Select(s => s.Specialty?.NameEn ?? s.Specialty?.NameAr)
                    .FirstOrDefault(n => !string.IsNullOrWhiteSpace(n));

                return new DeveloperBrowseDto
                {
                    UserId = d.UserId,
                    ProfileId = d.Id,
                    FirstName = d.User.FristName,
                    LastName = d.User.LastName,
                    ProfileImage = ResolveProfileImageUrl(d.ProfileImage),
                    Bio = d.Bio,
                    Title = title,
                    AverageRating = d.AverageRating,
                    RatingCount = d.RatingCount,
                    Country = d.User.Country ?? string.Empty,
                    Skills = d.UserSkills
                        .Select(s => s.Skill?.Name ?? string.Empty)
                        .Where(n => !string.IsNullOrWhiteSpace(n))
                        .Take(6)
                        .ToList(),
                    Categories = d.UserInterests
                        .Select(i => i.Category?.NameEn ?? i.Category?.Name ?? string.Empty)
                        .Where(n => !string.IsNullOrWhiteSpace(n))
                        .Take(4)
                        .ToList()
                };
            }).ToList();

            return Result.Success(PaginatedResult<DeveloperBrowseDto>.FromList(
                items,
                filter.PageNumber,
                filter.PageSize,
                total));
        }

        public async Task<Result<IReadOnlyList<DeveloperReviewDto>>> GetDeveloperReviewsAsync(
            Guid userId,
            CancellationToken ct = default)
        {
            if (userId == Guid.Empty)
                return Result.Failure<IReadOnlyList<DeveloperReviewDto>>(UserErrors.UserNotFound);

            var exists = await _developerProfileRepository.Query()
                .AnyAsync(d => d.UserId == userId && !d.IsDeleted, ct);
            if (!exists)
                return Result.Failure<IReadOnlyList<DeveloperReviewDto>>(UserErrors.UserNotFound);

            var reviewRepo = unitOfWork.Repository<IReviewRepository, Review>();
            var marketplace = await reviewRepo.GetByRevieweeAsync(RevieweeType.User, userId, ct);
            var community = await _developerProfileRepository.GetFeedbackAsync(userId, 50, ct);

            var mapped = marketplace.Select(MapMarketplaceReview)
                .Concat(community.Select(MapCommunityReview))
                .OrderByDescending(r => r.CreatedAt)
                .Take(20)
                .ToList();

            return Result.Success<IReadOnlyList<DeveloperReviewDto>>(mapped);
        }

        public async Task<Result<DeveloperReviewDto>> AddDeveloperReviewAsync(
            Guid developerUserId,
            CreateDeveloperReviewRequestDto request,
            CancellationToken ct = default)
        {
            if (developerUserId == Guid.Empty)
                return Result.Failure<DeveloperReviewDto>(UserErrors.UserNotFound);

            var reviewerId = currentUserService.UserId;
            if (reviewerId == Guid.Empty)
                return Result.Failure<DeveloperReviewDto>(
                    new Error("Auth.Unauthorized", "Unauthorized.", StatusCodes.Status401Unauthorized));

            if (reviewerId == developerUserId)
                return Result.Failure<DeveloperReviewDto>(
                    new Error("Review.Forbidden", "You cannot review yourself.", StatusCodes.Status403Forbidden));

            var exists = await _developerProfileRepository.ExistsForUserAsync(developerUserId, ct);
            if (!exists)
                return Result.Failure<DeveloperReviewDto>(UserErrors.UserNotFound);

            if (request.Rating is < 1 or > 5)
                return Result.Failure<DeveloperReviewDto>(
                    new Error("Validation.Failed", "Rating must be between 1 and 5.", StatusCodes.Status400BadRequest));

            var comment = string.IsNullOrWhiteSpace(request.Comment)
                ? null
                : request.Comment.Trim();

            if (comment is { Length: > 500 })
                return Result.Failure<DeveloperReviewDto>(
                    new Error("Validation.Failed", "Comment must be 500 characters or fewer.", StatusCodes.Status400BadRequest));

            if (await _developerProfileRepository.HasFeedbackAsync(developerUserId, reviewerId, ct))
                return Result.Failure<DeveloperReviewDto>(
                    new Error("Review.Conflict", "You already reviewed this developer.", StatusCodes.Status409Conflict));

            var (isMuted, mutedUntil) = await contentModerationService.GetMuteStatusAsync(reviewerId, ct);
            if (isMuted)
                return Result.Failure<DeveloperReviewDto>(
                    new Error("Moderation.Restricted",
                        $"You are temporarily restricted from posting reviews until {mutedUntil:u}.",
                        StatusCodes.Status403Forbidden));

            var now = DateTime.UtcNow;
            var feedback = new DeveloperFeedback
            {
                Id = Guid.NewGuid(),
                DeveloperUserId = developerUserId,
                ReviewerUserId = reviewerId,
                Rating = request.Rating,
                Comment = comment,
                CreatedAt = now,
                CreatedBy = reviewerId.ToString(),
                ModerationStatus = ModerationStatus.Visible
            };

            await _developerProfileRepository.AddFeedbackAsync(feedback, ct);
            await unitOfWork.SaveChangesAsync(ct);

            string? moderationWarning = null;
            if (!string.IsNullOrWhiteSpace(comment))
            {
                var active = await _userRepository.GetActiveProfileAsync(reviewerId, ct);
                Guid? clientProfileId = null;
                Guid? developerProfileId = null;
                if (active is not null)
                {
                    if (active.Value.Mode == profileMode.Client) clientProfileId = active.Value.ProfileId;
                    else developerProfileId = active.Value.ProfileId;
                }

                var moderation = await contentModerationService.ModerateAndEnforceAsync(
                    reviewerId,
                    ModerationSourceType.DeveloperFeedback,
                    feedback.Id,
                    comment,
                    "review",
                    clientProfileId,
                    developerProfileId,
                    ct);

                feedback.ModerationStatus = moderation.Status;
                feedback.ModerationNote = moderation.WarningMessage;
                feedback.ModeratedText = moderation.Status switch
                {
                    ModerationStatus.Visible => null,
                    ModerationStatus.Redacted => moderation.SafeText,
                    _ => "Review comment removed by FreeGency for a policy violation."
                };
                moderationWarning = moderation.WarningMessage;
                await unitOfWork.SaveChangesAsync(ct);

                if (moderation.Action == ModerationAction.BlockSubmit)
                {
                    return Result.Failure<DeveloperReviewDto>(
                        new Error("Moderation.Blocked",
                            moderation.WarningMessage ?? "This review violates FreeGency community guidelines.",
                            StatusCodes.Status422UnprocessableEntity));
                }
            }

            var allCommunity = await _developerProfileRepository.GetFeedbackAsync(developerUserId, 500, ct);
            var visibleCommunity = allCommunity
                .Where(f => f.ModerationStatus != ModerationStatus.Hidden)
                .ToList();
            var reviewRepo = unitOfWork.Repository<IReviewRepository, Review>();
            var marketplace = await reviewRepo.GetByRevieweeAsync(RevieweeType.User, developerUserId, ct);

            var allRatings = visibleCommunity.Select(f => f.Rating)
                .Concat(marketplace.Select(r => r.Rating))
                .ToList();
            var count = allRatings.Count;
            var average = count == 0 ? 0m : (decimal)allRatings.Average();
            await _developerProfileRepository.UpdateRatingAsync(
                developerUserId,
                Math.Round(average, 2),
                count,
                ct);

            var withUser = allCommunity.FirstOrDefault(x => x.Id == feedback.Id) ?? feedback;
            var dto = MapCommunityReview(withUser);
            dto.ModerationWarning = moderationWarning;
            return Result.Success(dto);
        }

        private DeveloperReviewDto MapMarketplaceReview(Review review)
            => MapReviewer(
                review.Id,
                review.Rating,
                review.Comment,
                review.CreatedAt,
                review.ReviewerUserId,
                review.ReviewerUser);

        private DeveloperReviewDto MapCommunityReview(DeveloperFeedback feedback)
        {
            var comment = feedback.ModerationStatus switch
            {
                ModerationStatus.Visible => feedback.Comment,
                ModerationStatus.Redacted => feedback.ModeratedText ?? feedback.Comment,
                _ => feedback.ModeratedText ?? "Review comment removed by FreeGency for a policy violation."
            };
            var dto = MapReviewer(
                feedback.Id,
                feedback.Rating,
                comment,
                feedback.CreatedAt,
                feedback.ReviewerUserId,
                feedback.ReviewerUser);
            dto.ModerationStatus = feedback.ModerationStatus.ToString();
            return dto;
        }

        private DeveloperReviewDto MapReviewer(
            Guid id,
            int rating,
            string? comment,
            DateTime createdAt,
            Guid reviewerUserId,
            User? user)
        {
            var name = user is null
                ? string.Empty
                : $"{user.FristName} {user.LastName}".Trim();

            if (string.IsNullOrWhiteSpace(name))
                name = user?.UserName?.Trim() ?? "Client";

            var avatar = user?.ClientProfile?.ProfileImage
                ?? user?.DeveloperProfile?.ProfileImage;

            return new DeveloperReviewDto
            {
                Id = id,
                Rating = rating,
                Comment = comment,
                CreatedAt = createdAt,
                ReviewerUserId = reviewerUserId,
                ReviewerName = name,
                ReviewerTitle = "Client",
                ReviewerAvatar = ResolveProfileImageUrl(avatar),
                ModerationStatus = ModerationStatus.Visible.ToString(),
            };
        }

        private string? ResolveProfileImageUrl(string? profileImage)
        {
            if (string.IsNullOrWhiteSpace(profileImage))
                return profileImage;

            return Uri.TryCreate(profileImage, UriKind.Absolute, out _)
                ? profileImage
                : currentUserService.origin + profileImage;
        }

      
    }
}
