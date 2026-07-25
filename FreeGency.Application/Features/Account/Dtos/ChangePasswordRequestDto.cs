using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.Account.Dtos
{
    public class ChangePasswordRequestDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
