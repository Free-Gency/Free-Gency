using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Helpers
{
    public class StripeSetting
    {
        public string PublishableKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        /// <summary>Dashboard / deployed webhook signing secret.</summary>
        public string WhSecret { get; set; } = string.Empty;
        /// <summary>Optional Stripe CLI secret from `stripe listen` (local only).</summary>
        public string? CliWhSecret { get; set; }
    }
}
