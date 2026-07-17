using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.DTOs.AuthenticationDtos
{
    public class RegisterRequestDto
    {
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string Email { get; init; }
        public string Password { get; init; }
        public string Country { get; init; }
        public string PhoneNumber { get; init; }
        public string Mode { get; init; }
    }
}
