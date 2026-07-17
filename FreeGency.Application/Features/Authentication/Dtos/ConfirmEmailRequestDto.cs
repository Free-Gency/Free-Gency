using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.Authentication.Dtos
{
    public class ConfirmEmailRequestDto
    {
        public string userId { get; set; }
        public string code { get; set; }
        
    }
}
