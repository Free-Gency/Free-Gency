using FreeGency.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Errors
{
    public static class SocialLinkError
    {
        public static readonly Error SocialLinkNotFound =
             new("Link.Social Link Not Found", " Social Link Not Found", StatusCodes.Status404NotFound);
    }
}
