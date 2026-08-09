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
        public static readonly Error ProfileNotFound =
          new("User. Profile Not Found", "Profile Not Found", StatusCodes.Status404NotFound);
        public static readonly Error DeveloperProfileAlreadyExists =
           new("User.Developer Profile Exists", "Developer Profile already exists", StatusCodes.Status409Conflict);
        public static readonly Error DeveloperProfileNotFound =
          new("User.Developer Profile Not Found", "Developer Profile Not Found", StatusCodes.Status404NotFound);
        public static readonly Error ClientProfileRequired =
           new("Profile.ClientRequired", "Create a Client profile before switching to Client mode.", StatusCodes.Status409Conflict);
        public static readonly Error DeveloperProfileRequired =
           new("Profile.DeveloperRequired", "Create a Developer profile before switching to Developer mode.", StatusCodes.Status409Conflict);
    }
}
