using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.DTOs.AuthenticationDtos
{
    public class RefreshTokenRequestDto
    {
        public string Token { get; init; }
        public string RefreshToken { get; init; }
    }
}
