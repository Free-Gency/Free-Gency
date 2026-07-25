using FreeGency.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Common.Errors
{
    public static class TeamErrors
    {
        public static readonly Error TeamJobNotFound =
                     new("TeamJob.NotFound", "The requested team job was not found.",StatusCodes.Status404NotFound);
        public static readonly Error TeamNotFound =
       new("TeamJob.NotFound", "The requested team job was not found.",StatusCodes.Status404NotFound);
        public static readonly Error AlreadyMember =
       new("Team.AlreadyMember", "You are already a member of this team.",StatusCodes.Status409Conflict);
        public static readonly Error JoinRequestAlreadyExists =
        new("Team.JoinRequestAlreadyExists", "You already have a pending join request.",StatusCodes.Status400BadRequest);
        public static readonly Error JoinRequestNotFound =
    new(
        "Team.JoinRequestNotFound",
        "The requested join request was not found.",
        StatusCodes.Status404NotFound);

        public static readonly Error RequestAlreadyHandled =
            new(
                "Team.RequestAlreadyHandled",
                "This join request has already been processed.",
                StatusCodes.Status409Conflict);

        public static readonly Error UnauthorizedLeader =
            new(
                "Team.UnauthorizedLeader",
                "You are not authorized to manage this team's join requests.",
                StatusCodes.Status403Forbidden);

        public static readonly Error TeamJobClosed =
            new(
                "Team.TeamJobClosed",
                "This team job is no longer accepting applications.",
                StatusCodes.Status400BadRequest);

        public static readonly Error TeamIsFull =
            new(
                "Team.TeamIsFull",
                "This team has reached its maximum number of members.",
                StatusCodes.Status409Conflict);
        public static readonly Error NotAuthorized =
    new(
        "Team.NotAuthorized",
        "You are not authorized to perform this action.",
        StatusCodes.Status403Forbidden);
    }
}
