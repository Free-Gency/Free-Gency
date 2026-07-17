using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FreeGency.Infrastructure.Implementations
{
    public class EmailBinding
    {
        public static string NameSection = "MailSettings";
        [Required]
        public string Mail { get; set; }
        [Required]
        public string DisplayName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Host { get; set; }
        [Required]
        public int Port { get; set; }
    }
}
