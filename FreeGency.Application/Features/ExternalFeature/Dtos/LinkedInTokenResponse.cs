using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.ExternalFeature.Dtos
{
    public class LinkedInTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = null!;

        [JsonPropertyName("id_token")]
        public string IdToken { get; set; } = null!;
    }
}
