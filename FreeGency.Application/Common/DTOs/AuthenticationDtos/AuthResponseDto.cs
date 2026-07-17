using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.DTOs.AuthenticationDtos
{
    public class AuthResponseDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Token { get; set; }
        public int ExpiresIn { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
    }
}
