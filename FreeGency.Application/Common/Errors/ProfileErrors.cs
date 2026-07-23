using FreeGency.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Errors
{
    public static class ProfileErrors
    {
        public static readonly Error ClientProfileAlreadyExists =
           new("User.Client Profile Exists", "Client Profile already exists", StatusCodes.Status409Conflict);
        public static readonly Error DeveloperProfileAlreadyExists =
           new("User.Developer Profile Exists", "Developer Profile already exists", StatusCodes.Status409Conflict);
    }
}
