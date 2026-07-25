using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.Account.Dtos
{
    public class UpdateClientAccountDto
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? Country { get; set; }

        public IFormFile? ProfileImage { get; set; }

        public string? Bio { get; set; }
    }
}
