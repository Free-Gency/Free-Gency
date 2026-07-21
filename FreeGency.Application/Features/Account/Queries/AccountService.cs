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

        public Task<Result> UpdateProfileAsync()
        {
            throw new NotImplementedException();
        }
    }
}
