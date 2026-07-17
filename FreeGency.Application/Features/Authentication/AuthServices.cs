using FoundIt.Application.Common.Models;
using FreeGency.Application.Common.DTOs.AuthenticationDtos;
using FreeGency.Application.Common.Errors;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Common.Mappings.AuthenticationMapping;
using FreeGency.Application.Common.Results;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace FreeGency.Application.Features.Authentication
{
    public class AuthServices(UserManager<User> userManager,IJwtProvider jwtProvider) : IAuthServices
    {
        private readonly int _refreshTokenExpiryDays = 14;

        public async Task<Result> RegisterAsync(RegisterRequestDto dto)
        {
            var emailIsExist = await userManager.Users.AnyAsync(x=>x.Email==dto.Email);
            if (emailIsExist) return Result.Failure(UserErrors.EmailAlreadyExists);
            var user = dto.ToEntity();
            var result = await userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return Result.Failure(new Error(result.Errors.First().Code, result.Errors.First().Description, StatusCodes.Status400BadRequest));
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

       
    }
}
