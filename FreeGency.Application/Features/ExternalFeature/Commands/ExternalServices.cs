using FreeGency.Application.Common.DTOs.AuthenticationDtos;
using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Common.Mappings.AuthenticationMapping;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces;
using FreeGency.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Security.Cryptography;

namespace FreeGency.Application.Features.ExternalFeature.Commands
{
    public class ExternalServices(
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        IJwtProvider jwtProvider,
        IUnitOfWork unitOfWork) : IExternalServices
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
                if (ResolveIntent(info) != "signup")
                    return Result.Failure<AuthResponseDto>(ExternalErrors.ExternalAccountNotFound);

                var profileModeValue = ResolveSignupMode(info);
                var givenName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                var surname = info.Principal.FindFirstValue(ClaimTypes.Surname);
                var fullName = info.Principal.FindFirstValue(ClaimTypes.Name);

                user = new User
                {
                    Email = email,
                    UserName = email,
                    EmailConfirmed = true,
                    ActiveProfileMode = profileModeValue,
                    FristName = !string.IsNullOrWhiteSpace(givenName)
                        ? givenName
                        : fullName?.Split(' ', 2)[0] ?? string.Empty,
                    LastName = !string.IsNullOrWhiteSpace(surname)
                        ? surname
                        : fullName?.Contains(' ') == true
                            ? fullName.Split(' ', 2)[1]
                            : string.Empty,
                };

                var result = await userManager.CreateAsync(user);

                if (!result.Succeeded)
                    return Result.Failure<AuthResponseDto>(ExternalErrors.ExternalLoginFailed);

                await userManager.AddLoginAsync(user, info);
                await EnsureProfileForModeAsync(user.Id, profileModeValue);
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

        private async Task EnsureProfileForModeAsync(Guid userId, profileMode mode)
        {
            if (mode == profileMode.Developer)
            {
                var repo = unitOfWork.Repository<IDeveloperProfileRepository, DeveloperProfile>();
                var existing = await repo.GetByUserIdAsync(userId);
                if (existing == null)
                {
                    await repo.AddAsync(new DeveloperProfile { UserId = userId });
                    await unitOfWork.SaveChangesAsync();
                }
                return;
            }

            var clientRepo = unitOfWork.Repository<IClientProfileRepository, ClientProfile>();
            var clientExisting = await clientRepo.GetByUserIdAsync(userId);
            if (clientExisting == null)
            {
                await clientRepo.AddAsync(new ClientProfile { UserId = userId });
                await unitOfWork.SaveChangesAsync();
            }
        }

        private static string ResolveIntent(Microsoft.AspNetCore.Identity.ExternalLoginInfo? info)
        {
            if (info?.AuthenticationProperties?.Items.TryGetValue("intent", out var intent) == true
                && intent == "signup")
                return "signup";

            return "login";
        }

        private static profileMode ResolveSignupMode(Microsoft.AspNetCore.Identity.ExternalLoginInfo? info)
        {
            if (info?.AuthenticationProperties?.Items.TryGetValue("signupMode", out var mode) != true)
                return profileMode.Client;

            return mode == "Developer" ? profileMode.Developer : profileMode.Client;
        }

        private static string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
    }
}
