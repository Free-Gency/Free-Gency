using FreeGency.Application.Common.Errors;
using FreeGency.Application.Features.SocialLinks.Dtos;
using FreeGency.Application.Features.SocialLinks.Mapping;
using FreeGency.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.SocialLinks.Commands
{
    public partial class SocialLinkService(ICurrentUserService currentUser,IUnitOfWork unitOfWork) : ISocialLinkService
    {
        private readonly ISocialLinkRepository _socialLinkRepository = unitOfWork.Repository<ISocialLinkRepository, SocialLink>();
        public async Task<Result> AddUserSocialLink(AddSocialUserLinkRequestDto dto)
        {
            var userId = currentUser.UserId;
            if (userId == Guid.Empty) return Result.Failure(UserErrors.UserNotFound);
            var socialLink = dto.ToEntity(userId);
            await _socialLinkRepository.AddAsync(socialLink);
            await unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
        public async Task<Result<List<SocialLinkUserDto>>> GetSocialLinksUser()
        {
            var userId = currentUser.UserId;
            if (userId == Guid.Empty) return Result.Failure<List<SocialLinkUserDto>>(UserErrors.UserNotFound);
            var spec = new SocialLinksUserSpecification(userId);
            var socialLinks = await _socialLinkRepository.ListAsync(spec);
            var Dtos = socialLinks.Select(x => x.ToDto()).ToList();
            return Result.Success(Dtos);
        }
        public async Task<Result> DeleteUserSocialLink(Guid Id)
        {
            var socialLink = await _socialLinkRepository.GetByIdAsync(Id);
            if (socialLink == null) return Result.Failure(SocialLinkError.SocialLinkNotFound);
             _socialLinkRepository.Delete(socialLink);
            await unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
        public async Task<Result> UpdateUserSocialLink(UpdateUserSocialLinkDto dto)
        {
            var socialLink = await _socialLinkRepository.GetByIdAsync(dto.Id);
            if (socialLink == null) return Result.Failure(SocialLinkError.SocialLinkNotFound);
            socialLink.UpdateEntity(dto);
            await unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
    }
}
