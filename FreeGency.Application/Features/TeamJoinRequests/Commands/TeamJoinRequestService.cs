using FreeGency.Application.Features.TeamJoinRequests.Dtos;
using FreeGency.Application.Features.TeamJoinRequests.Mapping;
using FreeGency.Domain.Interfaces.Repositories.Teams;
using FreeGency.Domain.Specifications;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeGency.Application.Features.TeamJoinRequests.Commands
{
    public partial class TeamJoinRequestService(IUnitOfWork unitOfWork,ICurrentUserService currentUserService) : ITeamJoinRequestService
    {
        private readonly ITeamJobRepository _teamJobRepository = unitOfWork.Repository<ITeamJobRepository, TeamJob>();
        private readonly ITeamMemberRepository _teamMemberRepository = unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
        private readonly ITeamJoinRequestRepository _teamJoinRequestRepository = unitOfWork.Repository<ITeamJoinRequestRepository, TeamJoinRequest>();
        private readonly ITeamRepository _teamRepository = unitOfWork.Repository<ITeamRepository, Team>();
        public async Task<Result> ApplyToTeamJobAsync(ApplyToTeamJobCommand applyToTeamJob)
        {
            var spec = new TeamJobSpecification(applyToTeamJob.JobId);
            var teamJob = await _teamJobRepository.GetEntityWithSpec(spec);
            if (teamJob == null) return Result.Failure(TeamErrors.TeamJobNotFound);

            var userId = currentUserService.UserId;
            var teamMemberSpec = new TeamMemberSpecification(teamJob.TeamId, userId);
            var teamMember = await _teamMemberRepository.GetEntityWithSpec(teamMemberSpec);
            if (teamMember != null) return Result.Failure(TeamErrors.AlreadyMember);

            var teamJobRequestSpec = new TeamJoinRequestSpecification(teamJob.TeamId, userId);
            var teamJobRequest = await _teamJoinRequestRepository.GetEntityWithSpec(teamJobRequestSpec);
            if (teamJobRequest != null) return Result.Failure(TeamErrors.JoinRequestAlreadyExists);

            var request = teamJob.ToEntity(userId, applyToTeamJob.CoverLetter);
            await _teamJoinRequestRepository.AddAsync(request);
            await unitOfWork.SaveChangesAsync();

            return Result.Success();
        }

       

        public async Task<Result> JoinByCodeAsync(JoinTeamByCodeCommand joinTeamByCode)
        {
            var teamSpec = new TeamSpecification(joinTeamByCode.code);
            var team = await _teamRepository.GetEntityWithSpec(teamSpec);
            if (team == null) return Result.Failure(TeamErrors.TeamNotFound);
            var userId = currentUserService.UserId;
            var teamMemberSpec = new TeamMemberSpecification(team.Id, userId);
            var teamMember = await _teamMemberRepository.GetEntityWithSpec(teamMemberSpec);
            if (teamMember != null) return Result.Failure(TeamErrors.AlreadyMember);
            var teamJobRequestSpec = new TeamJoinRequestSpecification(team.Id, userId);
            var teamJobRequest = await _teamJoinRequestRepository.GetEntityWithSpec(teamJobRequestSpec);
            if (teamJobRequest != null) return Result.Failure(TeamErrors.JoinRequestAlreadyExists);
            var request = team.ToEntity(userId, joinTeamByCode.CoverLetter);
            await _teamJoinRequestRepository.AddAsync(request);
            await unitOfWork.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> AcceptJoinRequestAsync(Guid requestId)
        {
            var request = await _teamJoinRequestRepository.GetEntityWithSpec(
                new TeamJoinRequestSpecification(requestId));

            if (request == null)
                return Result.Failure(TeamErrors.JoinRequestNotFound);

            var team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team is null)
                return Result.Failure(TeamErrors.TeamNotFound);

            var isLeader = await _teamMemberRepository.IsLeaderAsync(
                                                        request.TeamId,
                                                        currentUserService.UserId)
                || team.OwnerUserId == currentUserService.UserId;
            if (!isLeader)
                return Result.Failure(TeamErrors.NotAuthorized);
            if (request.Status != TeamJoinRequestStatus.pending)
                return Result.Failure(TeamErrors.RequestAlreadyHandled);

            var member = await _teamMemberRepository.GetEntityWithSpec(
                new TeamMemberSpecification(request.TeamId, request.UserId));

            if (member != null)
                return Result.Failure(TeamErrors.AlreadyMember);

            await _teamMemberRepository.AddAsync(new TeamMember
            {
                Id = Guid.NewGuid(),
                TeamId = request.TeamId,
                UserId = request.UserId,
                TeamRole = Role.TeamMember,
                Job = request.Job,
                JoinedAt = DateTime.UtcNow.ToString("O")
            });

            request.Status = TeamJoinRequestStatus.Accepted;
            request.ResponseAt = DateTime.UtcNow;
            request.RespondedByUserId = currentUserService.UserId.ToString();

            _teamJoinRequestRepository.Update(request);

            var chatRoomRepo = unitOfWork.Repository<IChatRoomRepository, ChatRoom>();
            var userRepo = unitOfWork.Repository<IUserRepository, User>();
            var mainRoom = await chatRoomRepo.GetTeamMainAsync(request.TeamId);
            if (mainRoom is not null)
            {
                var developerProfileId =
                    await userRepo.GetDeveloperProfileIdByUserIdAsync(request.UserId);
                if (developerProfileId is not null)
                {
                    await chatRoomRepo.AddMemberAsync(
                        mainRoom.Id,
                        clientProfileId: null,
                        developerProfileId: developerProfileId,
                        canSend: true,
                        roleLabel: "Team Member");
                }
            }

            await unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        public async Task<Result> RejectJoinRequestAsync(Guid requestId)
        {
            var request = await _teamJoinRequestRepository.GetEntityWithSpec(
                new TeamJoinRequestSpecification(requestId));

            if (request == null)
                return Result.Failure(TeamErrors.JoinRequestNotFound);

            var team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team is null)
                return Result.Failure(TeamErrors.TeamNotFound);

            var isLeader = await _teamMemberRepository.IsLeaderAsync(
                                                        request.TeamId,
                                                        currentUserService.UserId)
                || team.OwnerUserId == currentUserService.UserId;
            if (!isLeader)
                return Result.Failure(TeamErrors.NotAuthorized);
            if (request.Status != TeamJoinRequestStatus.pending)
                return Result.Failure(TeamErrors.RequestAlreadyHandled);

            request.Status = TeamJoinRequestStatus.Rejected;
            request.ResponseAt = DateTime.UtcNow;
            request.RespondedByUserId = currentUserService.UserId.ToString();

            _teamJoinRequestRepository.Update(request);

            await unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
