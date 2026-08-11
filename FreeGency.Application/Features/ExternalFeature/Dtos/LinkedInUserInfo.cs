using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.ExternalFeature.Dtos
{
    public class LinkedInUserInfo
    {
        [JsonPropertyName("sub")]
        public string Sub { get; set; } = null!;

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("given_name")]
        public string? GivenName { get; set; }

        [JsonPropertyName("family_name")]
        public string? FamilyName { get; set; }

        [JsonPropertyName("picture")]
        public string? Picture { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("email_verified")]
        public bool EmailVerified { get; set; }
    }
}
