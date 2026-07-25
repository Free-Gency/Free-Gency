using FreeGency.Application.Features.SocialLinks.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.SocialLinks.Mapping
{
    public static class SocialLinksMapping
    {
        public static SocialLinkUserDto ToDto(this SocialLink socialLink)
        {
            return new SocialLinkUserDto
            {
                Id=socialLink.Id,
                Platform = socialLink.Platform,
                Url = socialLink.Url
            };
        }
        public static SocialLink ToEntity(this AddSocialUserLinkRequestDto dto,Guid userId)
        {
            return new SocialLink
            {
                Id = Guid.NewGuid(),
                OwnerUserId = userId,
                Platform = dto.Platform,
                Url = dto.Url
            };
        }
        public static void UpdateEntity(this SocialLink socialLink,UpdateUserSocialLinkDto dto)
        {
            socialLink.Platform = dto.Platform;
            socialLink.Url = dto.Url;
            socialLink.UpdatedAt = DateTime.UtcNow;
        }
    }
}
