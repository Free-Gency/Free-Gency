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
        private readonly IClientProfileRepository clientProfileRepository = unitOfWork.Repository<IClientProfileRepository, ClientProfile>();
        private readonly IDeveloperProfileRepository developerProfileRepository = unitOfWork.Repository<IDeveloperProfileRepository, DeveloperProfile>();
        private readonly IWalletRepository walletRepository = unitOfWork.Repository<IWalletRepository, Wallet>();
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
                    HasCompletedOnboarding = false,
                    FristName = !string.IsNullOrWhiteSpace(givenName)
                        ? givenName
                        : fullName?.Split(' ', 2)[0] ?? string.Empty,
                    LastName = !string.IsNullOrWhiteSpace(surname)
                        ? surname
                        : fullName?.Contains(' ') == true
                            ? fullName.Split(' ', 2)[1]
                            : string.Empty,
                    refreshTokens = [],
                };

                var result = await userManager.CreateAsync(user);

                if (!result.Succeeded)
                    return Result.Failure<AuthResponseDto>(ExternalErrors.ExternalLoginFailed);
                var clientProfile = new ClientProfile { Id = Guid.NewGuid(), UserId = user.Id };
                var developerProfile = new DeveloperProfile { Id = Guid.NewGuid(), UserId = user.Id };
                var wallet = new Wallet { Id = Guid.NewGuid(), OwnerUserId = user.Id, OwnerType = owner.User, Currency = "USD" };
                await clientProfileRepository.AddAsync(clientProfile);
                await developerProfileRepository.AddAsync(developerProfile);
                await walletRepository.AddAsync(wallet);
                await unitOfWork.SaveChangesAsync();
                var loginResult = await userManager.AddLoginAsync(user, info);
                if (!loginResult.Succeeded)
                    return Result.Failure<AuthResponseDto>(ExternalErrors.ExternalLoginFailed);

                await EnsureProfileForModeAsync(user.Id, profileModeValue);
            }
            else
            {
                // Returning Google user — ensure login link + profile exist for their mode
                var logins = await userManager.GetLoginsAsync(user);
                if (logins.All(l => l.LoginProvider != info.LoginProvider))
                {
                    var loginResult = await userManager.AddLoginAsync(user, info);
                    if (!loginResult.Succeeded)
                        return Result.Failure<AuthResponseDto>(ExternalErrors.ExternalLoginFailed);
                }

                if (user.ActiveProfileMode is profileMode mode)
                    await EnsureProfileForModeAsync(user.Id, mode);
            }

            var (token, expiresIn) = jwtProvider.GenerateToken(user);
            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(14);

            // EF often materializes navigation collections as null when not included
            user.refreshTokens ??= [];
            user.refreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiresOn = refreshTokenExpiration,
            });

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return Result.Failure<AuthResponseDto>(ExternalErrors.ExternalLoginFailed);

            var authResponse = user.ToDto(token, expiresIn, refreshToken, refreshTokenExpiration);
            return Result.Success(authResponse);
        }

        private async Task EnsureProfileForModeAsync(Guid userId, profileMode mode)
        {
            if (mode == profileMode.Developer)
            {
                var repo = unitOfWork.Repository<IDeveloperProfileRepository, DeveloperProfile>();
                if (!await repo.ExistsForUserAsync(userId))
                {
                    await repo.AddAsync(new DeveloperProfile
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                    });
                    await unitOfWork.SaveChangesAsync();
                }
                return;
            }

            var clientRepo = unitOfWork.Repository<IClientProfileRepository, ClientProfile>();
            if (!await clientRepo.ExistsForUserAsync(userId))
            {
                await clientRepo.AddAsync(new ClientProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                });
                await unitOfWork.SaveChangesAsync();
            }
        }

        private static string ResolveIntent(ExternalLoginInfo? info)
        {
            if (info?.AuthenticationProperties?.Items.TryGetValue("intent", out var intent) == true
                && intent == "signup")
                return "signup";

            return "login";
        }

        private static profileMode ResolveSignupMode(ExternalLoginInfo? info)
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
