using FreeGency.Application.Features.SocialLinks.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Interfaces
{
    public interface ISocialLinkService
    {
        Task<Result<List<SocialLinkUserDto>>> GetSocialLinksUser();
        Task<Result> AddUserSocialLink(AddSocialUserLinkRequestDto dto);
        Task<Result> DeleteUserSocialLink(Guid Id);
        Task<Result> UpdateUserSocialLink(UpdateUserSocialLinkDto dto);
    }
}
