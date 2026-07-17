using FreeGency.Application.Common.DTOs.AuthenticationDtos;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Mappings.AuthenticationMapping
{
    public static class AuthMapping
    {
        public static AuthResponseDto ToDto(this User user,string token,int expiresIn,string refreshToken,DateTime refreshTokenEXpirationDays)
        {
            return new AuthResponseDto
            {
                Id=user.Id,
                Email = user.Email!,
                FirstName = user.FristName,
                LastName = user.LastName,
                Token = token,
                ExpiresIn = expiresIn,
                RefreshToken=refreshToken,
                RefreshTokenExpiration=refreshTokenEXpirationDays
            };
        }
        public static User ToEntity(this RegisterRequestDto dto)
        {
            return new User
            {
                Email = dto.Email,
                UserName = dto.Email,
                FristName = dto.FirstName,
                LastName = dto.LastName,
                ActiveProfileMode = dto.Mode == "Client" ? profileMode.Client : profileMode.Developer
            };
        }
    }
}
