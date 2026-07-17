using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.Authentication.Dtos
{
    public class ConfirmCodeRequestDto
    {
        public string code { get; init; }
        public string email { get; init; }
    }
}
