using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FreeGency.Application.Features.SocialLinks.Dtos
{
    public class SocialLinkUserDto
    {
        public Guid Id { get; set; }
        public string Platform { get; set; }
        public string Url { get; set; }
    }
}
