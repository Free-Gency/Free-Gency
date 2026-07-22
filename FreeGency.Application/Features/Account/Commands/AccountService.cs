using FreeGency.Application.Common.Errors;
using FreeGency.Application.Features.Account.Dtos;
using FreeGency.Application.Features.Account.Mapping;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Domain.Specifications;
using FreeGency.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.Account.Queries
{
    public partial class AccountService:IAccountService
    {
        public async Task<Result> UpdateClientProfileAsync(UpdateClientAccountDto dto)
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty) return Result.Failure(UserErrors.UserNotFound);
            var repo = unitOfWork.Repository<IClientProfileRepository, ClientProfile>();
            var spec = new ClientAccountSpecifiaction(userId);
            var clientAccount = await repo.GetEntityWithSpec(spec);
            if (clientAccount == null) return Result.Failure<ClientAccountResponseDto>(UserErrors.UserNotFound);
            clientAccount.UpdateToEntity(dto);
            if (dto.ProfileImage != null)
            {
                clientAccount.ProfileImage = await SaveImage(dto.ProfileImage, "ClientProfile");
            }

            repo.Update(clientAccount);
            await unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
        private async Task<string> SaveImage(IFormFile file, string type)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/Images/{type}");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return $"/Images/{type}/{uniqueFileName}";
        }
    }
}

