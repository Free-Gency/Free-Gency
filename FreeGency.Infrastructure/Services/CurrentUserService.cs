using FreeGency.Domain.Specifications;
using FreeGency.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Infrastructure.Services
{
    public class CurrentUserService(IHttpContextAccessor httpContextAccessor,IUnitOfWork unitOfWork): ICurrentUserService
    {
        private readonly IUserRepository userRepository = unitOfWork.Repository<IUserRepository, User>();
        private readonly IClientProfileRepository clientProfileRepository = unitOfWork.Repository<IClientProfileRepository, ClientProfile>();
        private readonly IDeveloperProfileRepository developerProfileRepository = unitOfWork.Repository<IDeveloperProfileRepository, DeveloperProfile>();
        public Guid UserId => Guid.TryParse(
      httpContextAccessor.HttpContext?.User?.FindFirst("uid")?.Value, out var userId) ? userId : Guid.Empty;

        public string origin =>
                    $"{httpContextAccessor.HttpContext?.Request.Scheme}://" +
                    $"{httpContextAccessor.HttpContext?.Request.Host}";

        public async Task<Guid> GetProfileId(Guid userId)
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user.ActiveProfileMode == profileMode.Client)
            {
                var client = await clientProfileRepository.GetEntityWithSpec(new ClientAccountSpecifiaction(user.Id));
                return client.Id;
            }
            else
            {
                var developer = await developerProfileRepository.GetEntityWithSpec(new DeveloperAccountSpecification(UserId));
                return developer.Id;
            }
        }
    }
}

