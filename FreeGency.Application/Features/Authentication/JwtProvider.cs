using FreeGency.Application.Common.Helpers;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FreeGency.Application.Features.Authentication
{
    public class JwtProvider(IOptions<JwtOptions> options) : IJwtProvider
    {
        private readonly JwtOptions _options = options.Value;

        public (string token, int expiresIn) GenerateToken(User user)
        {
            Claim[] claims =
                            [   
                new("uid", user.Id.ToString()),
                new(ClaimTypes.Email, user.Email!),
                new(ClaimTypes.GivenName, user.FristName),
                new(ClaimTypes.Surname, user.LastName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ];
            var symmetricsecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
            var signingCredentials = new SigningCredentials(symmetricsecurityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
               issuer: _options.Issuer,
               audience: _options.Audience,
               claims: claims,
               expires: DateTime.UtcNow.AddMinutes(_options.ExpiresIn),
               signingCredentials: signingCredentials
               );
            return (new JwtSecurityTokenHandler().WriteToken(token), _options.ExpiresIn * 60);
        }

        public Guid? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var symmetricsecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    IssuerSigningKey = symmetricsecurityKey,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
                var jwttoken = (JwtSecurityToken)validatedToken;
                return Guid.Parse(jwttoken.Claims.First(x => x.Type == "uid").Value);
            }
            catch
            {
                return null;
            }
        }
    }
}
