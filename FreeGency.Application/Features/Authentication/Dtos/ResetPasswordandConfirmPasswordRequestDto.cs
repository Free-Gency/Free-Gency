using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.Authentication.Dtos
{
    public class ResetPasswordandConfirmPasswordRequestDto
    {
        public string email { get; set; }
        public string password { get; set; }
        public string confirmPassword { get; set; }
    }
}
