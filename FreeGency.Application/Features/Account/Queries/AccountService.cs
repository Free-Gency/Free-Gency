using FreeGency.Application.Common.Errors;
using FreeGency.Application.Features.Account.Dtos;
using FreeGency.Application.Features.Account.Mapping;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Domain.Specifications;
using FreeGency.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.Account.Queries
{
    public partial class AccountService (ICurrentUserService currentUserService, IUnitOfWork unitOfWork, IStorageService storageService,UserManager<User> userManager): IAccountService
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
            var spec = new DeveloperAccountSpecification(userId, true);
            var developerProfile = await _developerProfileRepository.GetEntityWithSpec(spec);
            if (developerProfile is null)
                return Result.Failure<DeveloperAccountResponseDto>(UserErrors.UserNotFound);

            var response = developerProfile.ToDto();
            response.ProfileImage = ResolveProfileImageUrl(developerProfile.ProfileImage);
            return Result.Success(response);
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
