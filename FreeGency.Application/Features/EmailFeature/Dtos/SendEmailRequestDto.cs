using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.EmailFeature.Dtos
{
    public class SendEmailRequestDto
    {
        public string email { get; set; }
        public string message { get; set; }
    }
}
