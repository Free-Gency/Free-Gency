using FreeGency.Application.Features.TeamJoinRequests.Dtos;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.TeamJoinRequests.Mapping
{
    public static class TeamJobRequestMapping
    {
        public static TeamJoinRequest ToEntity(this TeamJob teamJob,Guid userId,string? cover)
        {
            return new TeamJoinRequest
            {
                Id = Guid.NewGuid(),
                TeamId = teamJob.TeamId,
                TeamJobId = teamJob.Id,
                UserId = userId,
                CoverLetter = cover,
                RequestedAt = DateTime.UtcNow,
                Job = teamJob.Title,
                CreatedBy = userId.ToString()
            };
        }
        public static TeamJoinRequest ToEntity(this Team team, Guid userId, string? coverLetter)
        {
            return new TeamJoinRequest
            {
                Id = Guid.NewGuid(),
                TeamId = team.Id,
                UserId = userId,
                CoverLetter = coverLetter,
                RequestedAt = DateTime.UtcNow,
                TeamJobId = null
            };
        }
        public static TeamJoinRequestResponseDto ToDto(this TeamJoinRequest request)
        {
            return new TeamJoinRequestResponseDto
            {
                Id = request.Id,

                UserId = request.UserId,
                FullName = $"{request.User.FristName} {request.User.LastName}",
                UserName = request.User.UserName,
                ProfilePicture = request.User.DeveloperProfile!.ProfileImage,

                AverageRating = request.User.DeveloperProfile.AverageRating,
                ReviewCount = 0,
                CompletedProjects = 0,

                TeamJobId = request.TeamJobId,
                TeamJobTitle = request.TeamJob?.Title,

                CoverLetter = request.CoverLetter,
                Status = request.Status,
                RequestedAt = request.RequestedAt
            };
        }
        public static UserRequestJoinResponseDto ToUserRequestJoinDto(this TeamJoinRequest request)
        {
            return new UserRequestJoinResponseDto
            {
                Id = request.Id,

                TeamId = request.TeamId,
                TeamName = request.Team.Name,
                TeamLogo = request.Team.Logo,

                TeamJobId = request.TeamJobId,
                TeamJobTitle = request.TeamJob?.Title,

                CoverLetter = request.CoverLetter,

                Status = request.Status,
                RequestedAt = request.RequestedAt
            };
        }
    }
}
