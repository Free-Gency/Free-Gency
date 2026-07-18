using FreeGency.Application.Common.DTOs.AuthenticationDtos;
using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Common.Mappings.AuthenticationMapping;
using FreeGency.Application.Common.Results;
using FreeGency.Application.Features.Authentication;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FreeGency.Application.Features.ExternalFeature.Commands
{
    public class ExternalServices (SignInManager<User> signInManager,UserManager<User> userManager,IJwtProvider jwtProvider,IAuthServices authServices): IExternalServices
    {
        public async Task<Result<AuthResponseDto>> LoginWithGoogleAsync()
        {
            var info = await signInManager.GetExternalLoginInfoAsync();

            if (info == null)
                return Result.Failure<AuthResponseDto>(ExternalErrors.ExternalLoginFailed);

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
                return Result.Failure<AuthResponseDto>(ExternalErrors.ExternalEmailNotFound);

            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new User
                {
                    Email = email,
                    UserName = email,
                    EmailConfirmed = true,
                    ActiveProfileMode = profileMode.Client
                };

                var result = await userManager.CreateAsync(user);

                if (!result.Succeeded)
                    return Result.Failure<AuthResponseDto>(ExternalErrors.ExternalLoginFailed);

                await userManager.AddLoginAsync(user, info);
            }
            var (token, expiresIn) = jwtProvider.GenerateToken(user);
            var refreshToken = GenerateRefreshToken();
            var refreshTokenEXpirationDays = DateTime.UtcNow.AddDays(14);
            user.refreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiresOn = refreshTokenEXpirationDays
            });
            await userManager.UpdateAsync(user);
            var authResponse = user.ToDto(token, expiresIn, refreshToken, refreshTokenEXpirationDays);
            return Result.Success(authResponse);
        }
        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
    }
}
