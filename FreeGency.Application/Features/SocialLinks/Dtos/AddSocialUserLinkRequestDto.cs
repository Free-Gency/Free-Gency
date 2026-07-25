using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FreeGency.Application.Features.SocialLinks.Dtos
{
    public class AddSocialUserLinkRequestDto
    {
        [Required]
        public string Platform { get; set; }
        [Required]
        public string Url { get; set; } 
    }
}
