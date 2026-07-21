using EntityFrameworkCore.EncryptColumn.Interfaces;
using FoundIt.Application.Common.Models;
using FreeGency.Application.Common.DTOs.AuthenticationDtos;
using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Common.Mappings.AuthenticationMapping;
using FreeGency.Application.Common.Results;
using FreeGency.Application.Features.Authentication.Dtos;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace FreeGency.Application.Features.Authentication
{
    public class AuthServices(UserManager<User> userManager,IJwtProvider jwtProvider
                              ,IHttpContextAccessor httpContextAccessor,IEmailService emailService,IEncryptionProvider encryptionProvider
                              ,Microsoft.Extensions.Configuration.IConfiguration configuration) : IAuthServices
    {
        private readonly int _refreshTokenExpiryDays = 14;

        public async Task<Result> RegisterAsync(RegisterRequestDto dto)
        {
            var emailIsExist = await userManager.Users.AnyAsync(x=>x.Email==dto.Email);
            if (emailIsExist) return Result.Failure(UserErrors.EmailAlreadyExists);
            var user = dto.ToEntity();
            var result = await userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return Result.Failure(new Error(result.Errors.First().Code, result.Errors.First().Description, StatusCodes.Status400BadRequest));
            // generate profiles
            //send comfirmaion email
            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(
                               Encoding.UTF8.GetBytes(code));
            var frontendUrl = configuration["FrontendUrl"]?.TrimEnd('/')
                ?? "http://localhost:4200";
            var ReturnUrl = $"{frontendUrl}/auth/confirm-email?userId={user.Id}&code={code}";
            //body
            var resultOfConfirmEmail = await emailService.SendMassege(user.Email!, ReturnUrl, "Confirm Your Email");
            if (!resultOfConfirmEmail)
                return Result.Failure<string>(AuthenticationErrors.ConfirmEmail);
            return Result.Success();
        }


        public async Task<Result<AuthResponseDto>?> GetTokenAsync(LoginRequestDto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Result.Failure<AuthResponseDto>(UserErrors.InvalidCredentials);
            var isValidPassword = await userManager.CheckPasswordAsync(user, dto.Password);
            if (!isValidPassword)
                return Result.Failure<AuthResponseDto>(UserErrors.InvalidCredentials);
            if (!user.EmailConfirmed) return Result.Failure<AuthResponseDto>(AuthenticationErrors.EmailUserNotConfirmed);
            var authResponse = await GetAuthResponseDto(user);
            
            return Result.Success(authResponse);   
        }


        public async Task<Result<AuthResponseDto>> GetRefeshTokenaync(string Token, string RefreshToken, CancellationToken cancellationToken = default)
        {
            var userId = jwtProvider.ValidateToken(Token);
            if (userId == null) return Result.Failure<AuthResponseDto>(AuthenticationErrors.TokenNotValid);
            var user = await userManager.FindByIdAsync(userId.ToString()!);
            if (user == null) return Result.Failure<AuthResponseDto>(AuthenticationErrors.TokenNotValid);
            var userRefreshToken = user.refreshTokens.SingleOrDefault(x => x.Token == RefreshToken && x.IsActive);
            if (userRefreshToken == null) return Result.Failure<AuthResponseDto>(AuthenticationErrors.RefreshTokenNotFound);
            userRefreshToken.RevokedOn = DateTime.UtcNow;
            var authResponseDto = await GetAuthResponseDto(user);
            return Result.Success(authResponseDto);
        }
        public async Task<Result<bool>> RevokeRefeshTokenaync(string Token, string RefreshToken, CancellationToken cancellationToken = default)
        {
            var userId = jwtProvider.ValidateToken(Token);
            if (userId == null) return Result.Failure<bool>(AuthenticationErrors.TokenNotValid);
            var user = await userManager.FindByIdAsync(userId.ToString()!);
            if (user == null) return Result.Failure<bool>(AuthenticationErrors.TokenNotValid);
            var userRefreshToken = user.refreshTokens.SingleOrDefault(x => x.Token == RefreshToken && x.IsActive);
            if (userRefreshToken == null) return Result.Failure<bool>(AuthenticationErrors.RefreshTokenNotFound);
            userRefreshToken.RevokedOn = DateTime.UtcNow;
            await userManager.UpdateAsync(user);
            return Result.Success(true);
        }
        private async Task<AuthResponseDto> GetAuthResponseDto(User user)
        {
            var (token, expiresIn) = jwtProvider.GenerateToken(user);
            var refreshToken = GenerateRefreshToken();
            var refreshTokenEXpirationDays = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);
            user.refreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiresOn = refreshTokenEXpirationDays
            });
            await userManager.UpdateAsync(user);
            var authResponse = user.ToDto(token, expiresIn, refreshToken, refreshTokenEXpirationDays);
            return authResponse;
        }
        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        public async Task<Result<string>> ComfirmEmail(ConfirmEmailRequestDto dto)
        {
            if (dto.code == null || dto.userId == null) return Result.Failure<string>(AuthenticationErrors.InvalidEmailConfirmationToken);
            dto.code = Encoding.UTF8.GetString(
                                       WebEncoders.Base64UrlDecode(dto.code));
            var user = await userManager.FindByIdAsync(dto.userId);
            if (user == null) return Result.Failure<string>(AuthenticationErrors.InvalidEmailConfirmationToken);
            var result = await userManager.ConfirmEmailAsync(user, dto.code);
            if (result.Succeeded) return Result.Success("Email Comfirmed");
            return Result.Failure<string>(AuthenticationErrors.InvalidEmailConfirmationToken);
        }

        public async Task<Result<string>> SendResetPasswordCode(ResetPasswordRequestDto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null) return Result.Failure<string>(UserErrors.UserNotFound);
            Random geerator = new Random();
            string randomNumber = geerator.Next(0, 1000000).ToString("D6");
            user.code = randomNumber;
            var result =await userManager.UpdateAsync(user);
            if (!result.Succeeded) return Result.Failure<string>(new Error(result.Errors.First().Code, result.Errors.First().Description, StatusCodes.Status400BadRequest));
            var message = "Code to Reset Password : " + randomNumber;
            await emailService.SendMassege(user.Email!, message, "Reset Password Code");
            return Result.Success("Code send successfly");
        }

        public async Task<Result> ComfirmCodeAsync(ConfirmCodeRequestDto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.email);
            if (user == null) return Result.Failure(UserErrors.UserNotFound);
            var usercode = user.code;
            if (dto.code != usercode) return Result.Failure(UserErrors.InvalidResetCode);
            user.code = null;
            await userManager.UpdateAsync(user);
            return Result.Success();
        }

        public async Task<Result<string>> ResetNewPassword(ResetPasswordandConfirmPasswordRequestDto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.email);
            if (user == null) return Result.Failure<string>(UserErrors.UserNotFound);
            await userManager.RemovePasswordAsync(user);
            await userManager.AddPasswordAsync(user, dto.password);
            return Result.Success("Password Reset Successfly");
        }
    }
}
