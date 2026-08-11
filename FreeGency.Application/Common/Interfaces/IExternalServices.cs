using FreeGency.Application.Common.DTOs.AuthenticationDtos;
using FreeGency.Application.Common.Results;
using FreeGency.Application.Features.ExternalFeature.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Interfaces
{
    public interface IExternalServices
    {
        Task<Result<AuthResponseDto>> LoginWithGoogleAsync();
        Task<Result<AuthResponseDto>> LoginWithLinkedInAsync(
      LinkedInUserInfo linkedInUser,
      string intent,
      string mode);
    }
}
