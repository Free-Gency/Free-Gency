using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.DTOs.AuthenticationDtos
{
    public class LoginRequestDto
    {
        public string Email { get; init; }
        public string Password { get; init; }
    }
}
