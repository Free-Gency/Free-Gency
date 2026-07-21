using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.Account.Dtos
{
    public class UpdateAccountDto
    {
        public IFormFile? ProfileImage { get; set; }
        public string? Bio { get; set; }
    }
}
