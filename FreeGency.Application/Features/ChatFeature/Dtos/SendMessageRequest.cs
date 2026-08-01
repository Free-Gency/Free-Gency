using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.ChatFeature.Dtos
{
    public class SendMessageRequest
    {
        public string? Text { get; set; }
        public IFormFile? File { get; set; }
    }
}
