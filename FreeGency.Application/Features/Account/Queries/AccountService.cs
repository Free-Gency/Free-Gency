using FreeGency.Application.Common.Errors;
using FreeGency.Application.Features.Account.Dtos;
using FreeGency.Application.Features.Account.Mapping;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Interfaces;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Domain.Specifications;
using FreeGency.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.Account.Queries
{
    public partial class AccountService (ICurrentUserService currentUserService,IUnitOfWork unitOfWork): IAccountService
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
            response.ProfileImage = currentUserService.origin + clientAccount.ProfileImage;
            return Result.Success(response);
        }

        public async Task<Result<DeveloperAccountResponseDto>> GetDeveloperProfile()
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty) return Result.Failure<DeveloperAccountResponseDto>(UserErrors.UserNotFound);
            var spec = new DeveloperAccountSpecification(userId, true);
            var developerProfile = await _developerProfileRepository.GetEntityWithSpec(spec);

            var response = developerProfile!.ToDto();
            return Result.Success(response);
        }
    }
}
