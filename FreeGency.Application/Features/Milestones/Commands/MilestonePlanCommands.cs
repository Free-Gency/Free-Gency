using FreeGency.Application.Features.ChatFeature.Dtos;
using FreeGency.Application.Features.Milestones.DTOs;
using FreeGency.Application.Features.NotificationFeature.Dtos;
using FreeGency.Domain.Constants;
using FreeGency.Domain.Interfaces.Repositories.Teams;
using Hangfire;
using Microsoft.AspNetCore.SignalR;

namespace FreeGency.Application.Features.Milestones.Commands;

public partial class MilestoneService
{
    private IMilestonePlanVersionRepository PlanRepo =>
        _unitOfWork.Repository<IMilestonePlanVersionRepository, MilestonePlanVersion>();

    private IEscrowHoldRepository EscrowRepo =>
        _unitOfWork.Repository<IEscrowHoldRepository, EscrowHold>();

    private IProjectProposalRepository ProposalRepo =>
        _unitOfWork.Repository<IProjectProposalRepository, ProjectProposal>();

    private IWalletRepository WalletRepo =>
        _unitOfWork.Repository<IWalletRepository, Wallet>();

    private ILedgerEntryRepository LedgerRepo =>
        _unitOfWork.Repository<ILedgerEntryRepository, LedgerEntry>();

    private ITeamMemberRepository TeamMemberRepo =>
        _unitOfWork.Repository<ITeamMemberRepository, TeamMember>();

    private ITeamPayoutSplitRepository SplitRepo =>
        _unitOfWork.Repository<ITeamPayoutSplitRepository, TeamPayoutSplit>();

    private IProjectEventRepository EventRepo =>
        _unitOfWork.Repository<IProjectEventRepository, ProjectEvent>();

    public async Task<ApiResponse<IEnumerable<MilestonePlanVersionDto>>> GetPlanVersionsAsync(
        Guid projectId, CancellationToken ct = default)
    {
        var project = await _projectRepo.GetByIdAsync(projectId, ct);
        if (project is null)
            return ApiResponse.Failure<IEnumerable<MilestonePlanVersionDto>>(AppError.NotFound(nameof(Project), projectId));

        if (!await CanAccessProjectMilestoneDataAsync(project, ct))
            return ApiResponse.Failure<IEnumerable<MilestonePlanVersionDto>>(
                AppError.Forbidden("You do not have access to this project's milestone plans."));

        var versions = await PlanRepo.GetByProjectIdAsync(projectId, ct);
        return ApiResponse.Success(versions.Select(MapPlanVersion));
    }

    public async Task<ApiResponse<MilestonePlanVersionDto>> GetLatestPlanAsync(
        Guid projectId, CancellationToken ct = default)
    {
        var project = await _projectRepo.GetByIdAsync(projectId, ct);
        if (project is null)
            return ApiResponse.Failure<MilestonePlanVersionDto>(AppError.NotFound(nameof(Project), projectId));

        if (!await CanAccessProjectMilestoneDataAsync(project, ct))
            return ApiResponse.Failure<MilestonePlanVersionDto>(
                AppError.Forbidden("You do not have access to this project's milestone plans."));

        var latest = await PlanRepo.GetLatestByProjectIdAsync(projectId, ct);
        if (latest is null)
            return ApiResponse.Failure<MilestonePlanVersionDto>(AppError.NotFound("MilestonePlan", projectId));

        return ApiResponse.Success(MapPlanVersion(latest));
    }

    public async Task<ApiResponse<MilestonePlanVersionDto>> ProposePlanAsync(
        ProposeMilestonePlanDto dto, CancellationToken ct = default)
    {
        if (dto.Milestones is null || dto.Milestones.Count == 0)
            return ApiResponse.Failure<MilestonePlanVersionDto>(AppError.Validation("At least one milestone is required."));

        if (dto.Milestones.Any(m => string.IsNullOrWhiteSpace(m.Title) || m.Amount <= 0))
            return ApiResponse.Failure<MilestonePlanVersionDto>(AppError.Validation("Each milestone needs a title and amount > 0."));

        var proposal = await ProposalRepo.GetByIdAsync(dto.ProposalId, ct);
        if (proposal is null)
            return ApiResponse.Failure<MilestonePlanVersionDto>(AppError.NotFound(nameof(ProjectProposal), dto.ProposalId));

        if (proposal.ProjectId != dto.ProjectId)
            return ApiResponse.Failure<MilestonePlanVersionDto>(AppError.Validation("Proposal does not belong to this project."));

        if (proposal.Status != ProposalStatus.InDiscussion)
            return ApiResponse.Failure<MilestonePlanVersionDto>(AppError.Validation("Proposal must be In Discussion to propose a plan."));

        var project = await _projectRepo.GetByIdAsync(dto.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure<MilestonePlanVersionDto>(AppError.NotFound(nameof(Project), dto.ProjectId));

        var budgetError = ValidatePlanTotalAgainstProjectBudget(
            project,
            dto.Milestones.Sum(m => m.Amount));
        if (budgetError is not null)
            return ApiResponse.Failure<MilestonePlanVersionDto>(budgetError);

        if (project.AssignedUserId is not null || project.AssignedTeamId is not null)
            return ApiResponse.Failure<MilestonePlanVersionDto>(AppError.Validation("Project already has a hired assignee."));

        if (!await IsProposalNegotiationSpeakerAsync(proposal, ct))
            return ApiResponse.Failure<MilestonePlanVersionDto>(AppError.Forbidden(
                "Only the team leader who submitted this proposal can propose a milestone plan."));

        var profileError = await RequireActiveProfileModeAsync(profileMode.Developer, ct);
        if (profileError is not null)
            return ApiResponse.Failure<MilestonePlanVersionDto>(profileError);

        var versionCount = await PlanRepo.CountByProjectIdAsync(dto.ProjectId, ct);
        if (versionCount >= MilestonePlanConstants.MaxPlanVersions)
            return ApiResponse.Failure<MilestonePlanVersionDto>(AppError.Validation(
                $"Plan version limit reached ({MilestonePlanConstants.MaxPlanVersions}). Close discussion or accept the latest plan."));

        var previous = await PlanRepo.GetLatestByProjectIdAsync(dto.ProjectId, ct);
        if (previous is not null && previous.Status == PlanVersionStatus.Accepted)
            return ApiResponse.Failure<MilestonePlanVersionDto>(AppError.Validation("A plan was already accepted for this project."));

        if (previous is not null && previous.Status == PlanVersionStatus.Proposed)
            return ApiResponse.Failure<MilestonePlanVersionDto>(AppError.Validation(
                "A plan is awaiting client response. Wait for Accept or Request Changes."));

        if (previous is not null && previous.ProposalId != dto.ProposalId)
            return ApiResponse.Failure<MilestonePlanVersionDto>(AppError.Validation(
                "Plan negotiation is tied to the active discussion proposal."));

        var nextVersion = versionCount + 1;
        var prevItems = previous?.Items.OrderBy(i => i.SortOrder).ToList() ?? [];

        var plan = new MilestonePlanVersion
        {
            Id = Guid.NewGuid(),
            ProjectId = dto.ProjectId,
            ProposalId = dto.ProposalId,
            Version = nextVersion,
            Status = PlanVersionStatus.Proposed,
            ProposedByUserId = _currentUser.UserId,
            Items = dto.Milestones.Select((m, index) =>
            {
                MilestoneChangeTag? tag = null;
                if (previous is not null)
                {
                    if (index >= prevItems.Count)
                        tag = MilestoneChangeTag.New;
                    else
                    {
                        var p = prevItems[index];
                        var changed = !string.Equals(p.Title, m.Title, StringComparison.Ordinal)
                                      || !string.Equals(p.DefinitionOfDone, m.DefinitionOfDone ?? string.Empty, StringComparison.Ordinal)
                                      || p.Amount != m.Amount
                                      || p.DueDate != m.DueDate;
                        if (changed) tag = MilestoneChangeTag.Updated;
                    }
                }

                return new MilestonePlanItem
                {
                    Id = Guid.NewGuid(),
                    Title = m.Title.Trim(),
                    DefinitionOfDone = (m.DefinitionOfDone ?? string.Empty).Trim(),
                    Amount = m.Amount,
                    DueDate = m.DueDate,
                    SortOrder = index + 1,
                    ChangeTag = tag
                };
            }).ToList()
        };

        await PlanRepo.AddAsync(plan, ct);

        var escrow = await EscrowRepo.GetByProjectIdAsync(dto.ProjectId, ct);
        if (escrow is null)
        {
            await EscrowRepo.AddAsync(new EscrowHold
            {
                Id = Guid.NewGuid(),
                ProjectId = dto.ProjectId,
                planStatus = PlanStatus.PlanSubmitted,
                FundingStatus = FundingStatus.Unlocked
            }, ct);
        }
        else
        {
            await EscrowRepo.UpdatePlanStatusAsync(dto.ProjectId, PlanStatus.PlanSubmitted, ct);
        }

        var proposalRoom = await ChatRoomRepo.GetByProposalIdForUpdateAsync(dto.ProposalId, ct);
        Message? planMessage = null;
        if (proposalRoom is not null)
        {
            var senderProfiles = await ResolveSenderProfilesForModeAsync(profileMode.Developer, ct);
            if (senderProfiles is null)
                return ApiResponse.Failure<MilestonePlanVersionDto>(
                    AppError.Validation("A Developer profile is required for chat."));

            planMessage = new Message
            {
                Id = Guid.NewGuid(),
                ChatRoomId = proposalRoom.Id,
                SenderClientProfileId = senderProfiles.Value.ClientProfileId,
                SenderDeveloperProfileId = senderProfiles.Value.DeveloperProfileId,
                MessageType = MessageType.MilestonePlan,
                Text = $"Milestone Plan v{nextVersion} proposed.",
                PlanVersionId = plan.Id,
                CreatedAt = DateTime.UtcNow
            };
            await MessageRepo.AddAsync(planMessage, ct);
            proposalRoom.UpdatedAt = DateTime.UtcNow;
        }

        await RecordEventAsync(project.Id, null, EventType.MilestonePlanProposed, null, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        var clientProfileId =
    await UserRepo.GetClientProfileIdByUserIdAsync(project.ClientId, ct);

        if (clientProfileId is null)
            return ApiResponse.Failure<MilestonePlanVersionDto>(
                AppError.Validation("Client profile was not found."));
        BackgroundJob.Enqueue(() =>
    _notificationService.CreateNotification(
        new CreateNotificationRequest
        {
            ClientProfileId = clientProfileId.Value,
            Title = "Milestone plan proposed",
            Body = $"A new milestone plan v{nextVersion} has been proposed for project {project.Title}.",
            Type = NotificationType.MilestonePlanProposed,
            ProjectId = project.Id,
            ProjectProposalId = proposal.Id,
            ActionUrl = $"/projects/{project.Id}?tab=milestones"
        }));
        // Broadcast must not fail the propose — plan is already committed.
        if (proposalRoom is not null && planMessage is not null)
        {
            try
            {
                await BroadcastChatMessageAsync(
                    proposalRoom.Id,
                    planMessage,
                    $"Milestone Plan v{nextVersion} proposed.",
                    ct);
            }
            catch
            {
                // Realtime notify is best-effort; client reloads the thread on success.
            }
        }

        var saved = await PlanRepo.GetByIdWithItemsAsync(plan.Id, ct);
        return ApiResponse.Success(
            MapPlanVersion(saved ?? plan),
            $"Milestone plan v{nextVersion} proposed.");
    }

    public async Task<ApiResponse> RequestPlanChangesAsync(RequestPlanChangesDto dto, CancellationToken ct = default)
    {
        var plan = await PlanRepo.GetByIdWithItemsAsync(dto.PlanVersionId, ct);
        if (plan is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(MilestonePlanVersion), dto.PlanVersionId));

        var project = await _projectRepo.GetByIdAsync(plan.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Project), plan.ProjectId));

        if (project.ClientId != _currentUser.UserId)
            return ApiResponse.Failure(AppError.Forbidden("Only the client can request plan changes."));

        var profileError = await RequireActiveProfileModeAsync(profileMode.Client, ct);
        if (profileError is not null)
            return ApiResponse.Failure(profileError);

        if (plan.Status != PlanVersionStatus.Proposed)
            return ApiResponse.Failure(AppError.Validation("Only a proposed plan can receive change requests."));

        if (string.IsNullOrWhiteSpace(dto.Comment))
            return ApiResponse.Failure(AppError.Validation("A general comment is required."));

        plan.Status = PlanVersionStatus.ChangesRequested;
        plan.ChangeComment = dto.Comment.Trim();
        PlanRepo.Update(plan);

        await EscrowRepo.UpdatePlanStatusAsync(plan.ProjectId, PlanStatus.PlanRevisionRequested, ct);

        var proposalRoom = await ChatRoomRepo.GetByProposalIdForUpdateAsync(plan.ProposalId, ct);
        Message? changeMessage = null;
        if (proposalRoom is not null)
        {
            var senderProfiles = await ResolveSenderProfilesForModeAsync(profileMode.Client, ct);
            if (senderProfiles is null)
                return ApiResponse.Failure(
                    AppError.Validation("A Client profile is required for chat."));

            changeMessage = new Message
            {
                Id = Guid.NewGuid(),
                ChatRoomId = proposalRoom.Id,
                SenderClientProfileId = senderProfiles.Value.ClientProfileId,
                SenderDeveloperProfileId = senderProfiles.Value.DeveloperProfileId,
                MessageType = MessageType.Text,
                Text = $"Request Changes on plan v{plan.Version}: {plan.ChangeComment}",
                CreatedAt = DateTime.UtcNow
            };
            await MessageRepo.AddAsync(changeMessage, ct);
            proposalRoom.UpdatedAt = DateTime.UtcNow;
        }

        await RecordEventAsync(project.Id, null, EventType.MilestonePlanChangesRequested, null, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        var proposerProfileId =
                            await UserRepo.GetDeveloperProfileIdByUserIdAsync(
                                plan.ProposedByUserId,
                                ct);

        if (proposerProfileId is not null)
        {
            BackgroundJob.Enqueue(() =>
                _notificationService.CreateNotification(
                    new CreateNotificationRequest
                    {
                        DeveloperProfileId = proposerProfileId.Value,
                        Title = "Milestone plan changes requested",
                        Body = $"The client requested changes on milestone plan v{plan.Version} for project {project.Title}.",
                        Type = NotificationType.MilestonePlanChangesRequested,
                        ProjectId = project.Id,
                        ProjectProposalId = plan.ProposalId,
                        ActionUrl = $"/projects/{project.Id}?tab=milestones"
                    }));
        }

        if (proposalRoom is not null && changeMessage is not null)
        {
            try
            {
                await BroadcastChatMessageAsync(
                    proposalRoom.Id,
                    changeMessage,
                    changeMessage.Text,
                    ct);
            }
            catch
            {
                // Best-effort realtime notify.
            }
        }

        return ApiResponse.Success("Changes requested. Waiting for a full revised plan version.");
    }

    /// <summary>Accept Milestone Plan = Hire. Cascade-rejects other proposals.</summary>
    public async Task<ApiResponse> AcceptPlanAsync(Guid planVersionId, CancellationToken ct = default)
    {
        var plan = await PlanRepo.GetByIdWithItemsAsync(planVersionId, ct);
        if (plan is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(MilestonePlanVersion), planVersionId));

        var project = await _projectRepo.GetByIdAsync(plan.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Project), plan.ProjectId));

        if (project.ClientId != _currentUser.UserId)
            return ApiResponse.Failure(AppError.Forbidden("Only the client can accept a milestone plan."));

        var profileError = await RequireActiveProfileModeAsync(profileMode.Client, ct);
        if (profileError is not null)
            return ApiResponse.Failure(profileError);

        if (plan.Status != PlanVersionStatus.Proposed)
            return ApiResponse.Failure(AppError.Validation("Only a proposed plan can be accepted."));

        var proposal = await ProposalRepo.GetByIdAsync(plan.ProposalId, ct);
        if (proposal is null || proposal.Status != ProposalStatus.InDiscussion)
            return ApiResponse.Failure(AppError.Validation("Linked proposal must be In Discussion."));

        var acceptBudgetError = ValidatePlanTotalAgainstProjectBudget(
            project,
            plan.Items.Sum(i => i.Amount));
        if (acceptBudgetError is not null)
            return ApiResponse.Failure(acceptBudgetError);

        // Materialize milestones from accepted plan
        var existing = (await _milestoneRepo.GetByProjectIdAsync(plan.ProjectId, ct)).ToList();
        foreach (var m in existing)
            _milestoneRepo.Delete(m);

        foreach (var item in plan.Items.OrderBy(i => i.SortOrder))
        {
            await _milestoneRepo.AddAsync(new Milestone
            {
                Id = Guid.NewGuid(),
                ProjectId = plan.ProjectId,
                Title = item.Title,
                Description = item.DefinitionOfDone,
                Amount = item.Amount,
                DueDate = item.DueDate,
                SortOrder = item.SortOrder,
                IsFunded = false,
                WorkStatus = WorkStatus.NotStarted,
                ReleaseStatus = ReleaseStatus.Locked,
                ProposedByUserId = plan.ProposedByUserId.ToString()
            }, ct);
        }

        plan.Status = PlanVersionStatus.Accepted;
        PlanRepo.Update(plan);

        await ProposalRepo.UpdateStatusAsync(proposal.Id, ProposalStatus.Accepted, ct);

        await _projectRepo.SetAssigneeAsync(
            plan.ProjectId,
            proposal.ApplicantType == ApplicantType.User ? proposal.UserId : null,
            proposal.ApplicantType == ApplicantType.Team ? proposal.TeamId : null,
            ct);

        await EscrowRepo.UpdatePlanStatusAsync(plan.ProjectId, PlanStatus.PlanAgreed, ct);

        // Reject cascade — only at Hire: every other open proposal becomes Rejected
        var rejectedOthers = (await ProposalRepo.GetCascadeRejectCandidatesAsync(plan.ProjectId, proposal.Id, ct)).ToList();
        foreach (var other in rejectedOthers)
        {
            await ProposalRepo.UpdateStatusAsync(
                other.Id,
                ProposalStatus.Rejected,
                MilestonePlanConstants.HiredAnotherCandidateReason,
                ct);

            var otherRoom = await ChatRoomRepo.GetByProposalIdForUpdateAsync(other.Id, ct);
            if (otherRoom is not null && otherRoom.Status == ChatRoomStatus.Active)
            {
                await MessageRepo.AddAsync(new Message
                {
                    Id = Guid.NewGuid(),
                    ChatRoomId = otherRoom.Id,
                    MessageType = MessageType.System,
                    Text = "Discussion closed — another candidate was hired."
                }, ct);
                otherRoom.Status = ChatRoomStatus.Archived;
                otherRoom.ArchivedAt = DateTime.UtcNow;
                ChatRoomRepo.Update(otherRoom);
            }
        }

        // Pending invites are no longer relevant after hire.
        var invitationRepo = _unitOfWork.Repository<IProjectInvitationRepository, ProjectInvitation>();
        var pendingInvites = await invitationRepo.GetPendingByProjectIdAsync(plan.ProjectId, ct);
        foreach (var invite in pendingInvites)
        {
            invite.Status = ProjectInvitationStatus.Cancelled;
            invite.RespondedAt = DateTime.UtcNow;
            invite.RespondedByUserId = _currentUser.UserId;
            invite.UpdatedAt = DateTime.UtcNow;
            invite.UpdatedBy = _currentUser.UserId.ToString();
            invitationRepo.Update(invite);
        }

        var proposalRoom = await ChatRoomRepo.GetByProposalIdForUpdateAsync(proposal.Id, ct);
        if (proposalRoom is not null)
        {
            await MessageRepo.AddAsync(new Message
            {
                Id = Guid.NewGuid(),
                ChatRoomId = proposalRoom.Id,
                MessageType = MessageType.System,
                Text = $"Milestone Plan v{plan.Version} accepted — hire locked. Negotiation chat archived."
            }, ct);

            proposalRoom.Status = ChatRoomStatus.Archived;
            proposalRoom.ArchivedAt = DateTime.UtcNow;
            // Free the unique ProjectId index so the new Project room can claim it.
            proposalRoom.ProjectId = null;
            ChatRoomRepo.Update(proposalRoom);
        }

        var clientProfileId = await UserRepo.GetClientProfileIdByUserIdAsync(project.ClientId, ct);
        if (clientProfileId is null)
            return ApiResponse.Failure(AppError.Validation("Client profile is required to create the project chat."));

        var projectMembers = new List<(Guid? ClientProfileId, Guid? DeveloperProfileId, bool CanSend, string? RoleLabel)>
        {
            (clientProfileId, null, true, "Client")
        };

        if (proposal.ApplicantType == ApplicantType.User && proposal.UserId.HasValue)
        {
            var developerProfileId =
                await UserRepo.GetDeveloperProfileIdByUserIdAsync(proposal.UserId.Value, ct);
            if (developerProfileId is null)
                return ApiResponse.Failure(AppError.Validation("Assignee developer profile was not found."));

            projectMembers.Add((null, developerProfileId, true, null));
        }
        else if (proposal.ApplicantType == ApplicantType.Team && proposal.TeamId.HasValue)
        {
            var leaders = await TeamMemberRepo.GetLeadersAsync(proposal.TeamId.Value, ct);
            var addedDeveloperProfileIds = new HashSet<Guid>();

            foreach (var leader in leaders)
            {
                var developerProfileId =
                    await UserRepo.GetDeveloperProfileIdByUserIdAsync(leader.UserId, ct);
                if (developerProfileId is null)
                    return ApiResponse.Failure(AppError.Validation("Team leader developer profile was not found."));

                projectMembers.Add((null, developerProfileId, true, "Team Leader"));
                addedDeveloperProfileIds.Add(developerProfileId.Value);
            }

            if (proposal.UserId.HasValue)
            {
                var speakerProfileId =
                    await UserRepo.GetDeveloperProfileIdByUserIdAsync(proposal.UserId.Value, ct);
                if (speakerProfileId is null)
                    return ApiResponse.Failure(AppError.Validation("Speaker developer profile was not found."));

                if (addedDeveloperProfileIds.Add(speakerProfileId.Value))
                    projectMembers.Add((null, speakerProfileId, true, "Team Leader"));
            }
        }

        var existingProjectRoom = await ChatRoomRepo.GetByProjectIdAsync(plan.ProjectId, ct);
        if (existingProjectRoom is null)
        {
            var projectRoom = new ChatRoom
            {
                RoomType = RoomType.Project,
                Status = ChatRoomStatus.Active,
                ProjectId = plan.ProjectId,
                TeamId = proposal.TeamId,
                ProposalId = null,
                SourceProposalRoomId = proposalRoom?.Id,
                Title = project.Title,
                CreatedByUserId = _currentUser.UserId
            };

            await ChatRoomRepo.AddWithMembersAsync(projectRoom, projectMembers, ct);

            await MessageRepo.AddAsync(new Message
            {
                Id = Guid.NewGuid(),
                ChatRoomId = projectRoom.Id,
                MessageType = MessageType.System,
                Text = $"Project started|{project.Title}|Milestone plan agreed · Team leaders can add working members"
            }, ct);
        }

        await RecordEventAsync(project.Id, null, EventType.MilestonePlanAgreed, null, ct);
        await RecordEventAsync(project.Id, null, EventType.ProposalAccepted, null, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await NotifyProposalApplicantsAsync(
            proposal,
            title: "Proposal accepted",
            body: $"Your proposal for \"{project.Title}\" was accepted. You've been hired — open milestones to continue.",
            type: NotificationType.ProposalAccepted,
            projectId: project.Id,
            proposalId: proposal.Id,
            actionUrl: $"/projects/{project.Id}?tab=milestones",
            ct);

        foreach (var other in rejectedOthers)
        {
            await NotifyProposalApplicantsAsync(
                other,
                title: "Proposal rejected",
                body: $"Your proposal for \"{project.Title}\" was rejected. {MilestonePlanConstants.HiredAnotherCandidateReason}.",
                type: NotificationType.ProposalRejected,
                projectId: project.Id,
                proposalId: other.Id,
                actionUrl: "/developer/manage-work",
                ct);
        }

        return ApiResponse.Success("Milestone plan accepted — hire complete. Fund Milestone #1 to start work.");
    }

    public async Task<ApiResponse> FundNextMilestoneAsync(Guid projectId, CancellationToken ct = default)
    {
        var project = await _projectRepo.GetByIdAsync(projectId, ct);
        if (project is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Project), projectId));

        if (project.ClientId != _currentUser.UserId)
            return ApiResponse.Failure(AppError.Forbidden("Only the client can fund milestones."));

        var profileError = await RequireActiveProfileModeAsync(profileMode.Client, ct);
        if (profileError is not null)
            return ApiResponse.Failure(profileError);

        var escrow = await EscrowRepo.GetByProjectIdAsync(projectId, ct);
        if (escrow is null || escrow.planStatus != PlanStatus.PlanAgreed)
            return ApiResponse.Failure(AppError.Validation("Accept a milestone plan before funding."));

        // Progressive: only one funded-but-unreleased milestone at a time
        var milestones = (await _milestoneRepo.GetByProjectIdAsync(projectId, ct)).ToList();
        if (milestones.Any(m => m.IsFunded && m.ReleaseStatus != ReleaseStatus.Released))
            return ApiResponse.Failure(AppError.Validation(
                "Release the current funded milestone before funding the next one."));

        var next = await _milestoneRepo.GetNextUnfundedAsync(projectId, ct);
        if (next is null)
            return ApiResponse.Failure(AppError.Validation("No unfunded milestones remaining."));

        var clientWallet = await WalletRepo.GetByOwnerAsync(owner.User, project.ClientId, ct);
        if (clientWallet is null)
            return ApiResponse.Failure(AppError.Validation("Client wallet not found."));

        if (clientWallet.Available < next.Amount)
            return ApiResponse.Failure(AppError.Validation("Insufficient wallet balance to fund this milestone."));

        var idempotencyKey = $"escrow-lock:{next.Id}";
        if (await LedgerRepo.ExistsByIdempotencyKeyAsync(idempotencyKey, ct))
            return ApiResponse.Failure(AppError.Validation("This milestone was already funded."));

        clientWallet.Available -= next.Amount;
        clientWallet.Reserved += next.Amount;
        WalletRepo.Update(clientWallet);

        next.IsFunded = true;
        next.WorkStatus = WorkStatus.InProgress;
        _milestoneRepo.Update(next);

        // Reload tracked escrow (GetByProjectId is AsNoTracking)
        var escrowTracked = await EscrowRepo.GetByIdAsync(escrow.Id, ct);
        if (escrowTracked is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(EscrowHold), escrow.Id));

        escrowTracked.TotalAmount += next.Amount;
        escrowTracked.FundingStatus = FundingStatus.Locked;
        escrowTracked.LockedAt ??= DateTime.UtcNow;
        EscrowRepo.Update(escrowTracked);

        await LedgerRepo.AddAsync(new LedgerEntry
        {
            Id = Guid.NewGuid(),
            WalletId = clientWallet.Id,
            EntryType = EntryType.EscrowLock,
            Amount = next.Amount,
            Currency = clientWallet.Currency,
            ProjectId = projectId,
            MilestoneId = next.Id,
            IdempotencyKey = idempotencyKey
        }, ct);

        await RecordEventAsync(projectId, next.Id, EventType.EscrowLocked, null, ct);

        var chatQueued = await TryQueueProjectMilestoneChatAsync(
            project,
            next,
            MessageType.MilestoneFunded,
            $"Milestone #{next.SortOrder} funded · ${next.Amount:0.##} · work can start",
            profileMode.Client,
            ct);

        await _unitOfWork.SaveChangesAsync(ct);
        await TryBroadcastQueuedChatAsync(
            chatQueued,
            $"Milestone #{next.SortOrder} funded",
            ct);

        if (project.AssignedUserId.HasValue)
        {
            var developerProfileId =
                await UserRepo.GetDeveloperProfileIdByUserIdAsync(
                    project.AssignedUserId.Value,
                    ct);

            if (developerProfileId is not null)
            {
                BackgroundJob.Enqueue(() =>
                    _notificationService.CreateNotification(
                        new CreateNotificationRequest
                        {
                            DeveloperProfileId = developerProfileId.Value,
                            Title = "Milestone funded",
                            Body = $"Milestone #{next.SortOrder} has been funded and work can start.",
                            Type = NotificationType.MilestoneFunded,
                            ProjectId = project.Id,
                            ActionUrl = $"/projects/{project.Id}?tab=milestones"
                        }));
            }
        }
        else if (project.AssignedTeamId.HasValue)
        {
            var leaders =
                await TeamMemberRepo.GetLeadersAsync(
                    project.AssignedTeamId.Value,
                    ct);

            foreach (var leader in leaders)
            {
                var developerProfileId =
                    await UserRepo.GetDeveloperProfileIdByUserIdAsync(
                        leader.UserId,
                        ct);

                if (developerProfileId is null)
                    continue;

                BackgroundJob.Enqueue(() =>
                    _notificationService.CreateNotification(
                        new CreateNotificationRequest
                        {
                            DeveloperProfileId = developerProfileId.Value,
                            Title = "Milestone funded",
                            Body = $"Milestone #{next.SortOrder} has been funded and work can start.",
                            Type = NotificationType.MilestoneFunded,
                            ProjectId = project.Id,
                            ActionUrl = $"/projects/{project.Id}?tab=milestones"
                        }));
            }
        }
        return ApiResponse.Success($"Milestone #{next.SortOrder} funded in escrow (${next.Amount}).");
    }

    public async Task<ApiResponse> SubmitMilestoneAsync(
        Guid milestoneId,
        string? note = null,
        CancellationToken ct = default)
    {
        // Check for open tasks before submission
        var openTasks = await _unitOfWork.Repository<ITaskRepository, ProjectTask>().CountIncompleteAsync(milestoneId, ct);
        var hasTasks = await _unitOfWork.Repository<ITaskRepository, ProjectTask>().Query()
            .AnyAsync(t => t.MilestoneId == milestoneId, ct);
        if (hasTasks && openTasks > 0)
            return ApiResponse.Failure(AppError.Validation(
                "Complete all tasks before submitting the milestone."));


        var milestone = await _milestoneRepo.GetByIdAsync(milestoneId, ct);
        if (milestone is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Milestone), milestoneId));

        var project = await _projectRepo.GetByIdAsync(milestone.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Project), milestone.ProjectId));

        if (!await IsProjectAssigneeAsync(project, ct))
            return ApiResponse.Failure(AppError.Forbidden("Only the hired assignee can submit a milestone."));

        var profileError = await RequireActiveProfileModeAsync(profileMode.Developer, ct);
        if (profileError is not null)
            return ApiResponse.Failure(profileError);

        if (!milestone.IsFunded)
            return ApiResponse.Failure(AppError.Validation("Milestone must be funded before submission."));

        if (milestone.WorkStatus is not (WorkStatus.InProgress or WorkStatus.ChangesRequested))
            return ApiResponse.Failure(AppError.Validation("Milestone is not ready for submission."));

        milestone.WorkStatus = WorkStatus.Submitted;
        milestone.SubmittedAt = DateTime.UtcNow;
        milestone.ReleaseStatus = ReleaseStatus.Pending;
        milestone.AvailableAt = DateTime.UtcNow;
        _milestoneRepo.Update(milestone);

        await RecordEventAsync(project.Id, milestone.Id, EventType.MilestoneSubmitted, null, ct);

        var trimmedNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        var chatText = trimmedNote is null
            ? $"Milestone #{milestone.SortOrder} submitted for review: {milestone.Title}"
            : $"Milestone #{milestone.SortOrder} submitted for review: {milestone.Title}\n\n{trimmedNote}";

        var chatQueued = await TryQueueProjectMilestoneChatAsync(
            project,
            milestone,
            MessageType.WorkSubmitted,
            chatText,
            profileMode.Developer,
            ct);

        await _unitOfWork.SaveChangesAsync(ct);
        await TryBroadcastQueuedChatAsync(
            chatQueued,
            $"Milestone #{milestone.SortOrder} submitted for review",
            ct);

        var clientProfileId =
    await UserRepo.GetClientProfileIdByUserIdAsync(
        project.ClientId,
        ct);

        if (clientProfileId is not null)
        {
            BackgroundJob.Enqueue(() =>
                _notificationService.CreateNotification(
                    new CreateNotificationRequest
                    {
                        ClientProfileId = clientProfileId.Value,
                        Title = "Milestone submitted",
                        Body = $"Milestone #{milestone.SortOrder} has been submitted for your review.",
                        Type = NotificationType.MilestoneSubmitted,
                        ProjectId = project.Id,
                        ActionUrl = $"/projects/{project.Id}?tab=milestones"
                    }));
        }
        return ApiResponse.Success(
            $"Milestone submitted. Client has {MilestonePlanConstants.AutoReleaseDays} days before auto-release.");
    }

    public async Task<ApiResponse> ApproveAndReleaseAsync(Guid milestoneId, CancellationToken ct = default)
    {
        var milestone = await _milestoneRepo.GetByIdAsync(milestoneId, ct);
        if (milestone is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Milestone), milestoneId));

        var project = await _projectRepo.GetByIdAsync(milestone.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Project), milestone.ProjectId));

        if (project.ClientId != _currentUser.UserId)
            return ApiResponse.Failure(AppError.Forbidden("Only the client can approve and release funds."));

        var profileError = await RequireActiveProfileModeAsync(profileMode.Client, ct);
        if (profileError is not null)
            return ApiResponse.Failure(profileError);

        if (milestone.WorkStatus != WorkStatus.Submitted || milestone.ReleaseStatus != ReleaseStatus.Pending)
            return ApiResponse.Failure(AppError.Validation("Milestone is not awaiting approval."));

        try
        {
            await ReleaseFundsInternalAsync(project, milestone, ct);
            var allOtherMilestonesApproved =
       await _milestoneRepo.AreAllOtherMilestonesApprovedAsync(
           project.Id,
           milestone.Id,
           ct);

            if (allOtherMilestonesApproved)
            {
                project.Status = ProjectStatus.Completed;
            }
        }
        catch (InvalidOperationException ex)
        {
            return ApiResponse.Failure(AppError.Validation(ex.Message));
        }
       

        await RecordEventAsync(project.Id, milestone.Id, EventType.MilestoneApproved, null, ct);
        await RecordEventAsync(project.Id, milestone.Id, EventType.MilestoneReleased, null, ct);

        var chatQueued = await TryQueueProjectMilestoneChatAsync(
            project,
            milestone,
            MessageType.MilestoneReleased,
            $"Milestone #{milestone.SortOrder} approved · ${milestone.Amount:0.##} released",
            profileMode.Client,
            ct);

        var completionChat = await TryCompleteProjectIfAllMilestonesReleasedAsync(project, milestone, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        await TryBroadcastQueuedChatAsync(
            chatQueued,
            $"Milestone #{milestone.SortOrder} approved & released",
            ct);
        await TryBroadcastQueuedChatAsync(
            completionChat,
            "Project completed · chat archived",
            ct);

        if (project.AssignedUserId.HasValue)
        {
            var developerProfileId =
                await UserRepo.GetDeveloperProfileIdByUserIdAsync(
                    project.AssignedUserId.Value,
                    ct);

            if (developerProfileId is not null)
            {
                BackgroundJob.Enqueue(() =>
                    _notificationService.CreateNotification(
                        new CreateNotificationRequest
                        {
                            DeveloperProfileId = developerProfileId.Value,
                            Title = "Payment released",
                            Body = $"Milestone #{milestone.SortOrder} was approved and ${milestone.Amount} has been released.",
                            Type = NotificationType.MilestoneReleased,
                            ProjectId = project.Id,
                            ActionUrl = $"/projects/{project.Id}?tab=milestones"
                        }));
            }
        }
        else if (project.AssignedTeamId.HasValue)
        {
            var leaders =
                await TeamMemberRepo.GetLeadersAsync(
                    project.AssignedTeamId.Value,
                    ct);

            foreach (var leader in leaders)
            {
                var developerProfileId =
                    await UserRepo.GetDeveloperProfileIdByUserIdAsync(
                        leader.UserId,
                        ct);

                if (developerProfileId is null)
                    continue;

                BackgroundJob.Enqueue(() =>
                    _notificationService.CreateNotification(
                        new CreateNotificationRequest
                        {
                            DeveloperProfileId = developerProfileId.Value,
                            Title = "Payment released",
                            Body = $"Milestone #{milestone.SortOrder} was approved and ${milestone.Amount} has been released.",
                            Type = NotificationType.MilestoneReleased,
                            ProjectId = project.Id,
                            ActionUrl = $"/projects/{project.Id}?tab=milestones"
                        }));
            }
        }
        return ApiResponse.Success(project.Status == ProjectStatus.Completed
            ? "Funds released. Project completed and chat archived."
            : "Funds released from escrow to the assignee.");
    }

    public async Task<ApiResponse> RequestMilestoneWorkChangesAsync(
        Guid milestoneId, string comment, CancellationToken ct = default)
    {
        var milestone = await _milestoneRepo.GetByIdAsync(milestoneId, ct);
        if (milestone is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Milestone), milestoneId));

        var project = await _projectRepo.GetByIdAsync(milestone.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Project), milestone.ProjectId));

        if (project.ClientId != _currentUser.UserId)
            return ApiResponse.Failure(AppError.Forbidden("Only the client can request work changes."));

        var profileError = await RequireActiveProfileModeAsync(profileMode.Client, ct);
        if (profileError is not null)
            return ApiResponse.Failure(profileError);

        if (milestone.WorkStatus != WorkStatus.Submitted)
            return ApiResponse.Failure(AppError.Validation("Only submitted milestones can receive change requests."));

        milestone.WorkStatus = WorkStatus.ChangesRequested;
        milestone.ReleaseStatus = ReleaseStatus.Locked;
        milestone.AvailableAt = null;
        _milestoneRepo.Update(milestone);
        await RecordEventAsync(project.Id, milestone.Id, EventType.MilestoneChangesRequested, null, ct);

        var trimmed = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        var chatText = trimmed is null
            ? $"Changes requested on milestone #{milestone.SortOrder}: {milestone.Title}"
            : $"Changes requested on milestone #{milestone.SortOrder}: {trimmed}";

        var chatQueued = await TryQueueProjectMilestoneChatAsync(
            project,
            milestone,
            MessageType.WorkChangesRequested,
            chatText,
            profileMode.Client,
            ct);

        await _unitOfWork.SaveChangesAsync(ct);
        await TryBroadcastQueuedChatAsync(
            chatQueued,
            $"Changes requested on milestone #{milestone.SortOrder}",
            ct);

        var notificationBody = string.IsNullOrWhiteSpace(comment)
    ? $"Changes were requested on milestone #{milestone.SortOrder}."
    : $"Changes were requested on milestone #{milestone.SortOrder}: {comment.Trim()}";

        if (project.AssignedUserId.HasValue)
        {
            var developerProfileId =
                await UserRepo.GetDeveloperProfileIdByUserIdAsync(
                    project.AssignedUserId.Value,
                    ct);

            if (developerProfileId is not null)
            {
                BackgroundJob.Enqueue(() =>
                    _notificationService.CreateNotification(
                        new CreateNotificationRequest
                        {
                            DeveloperProfileId = developerProfileId.Value,
                            Title = "Milestone changes requested",
                            Body = notificationBody,
                            Type = NotificationType.MilestoneChangesRequested,
                            ProjectId = project.Id,
                            ActionUrl = $"/projects/{project.Id}?tab=milestones"
                        }));
            }
        }
        else if (project.AssignedTeamId.HasValue)
        {
            var leaders =
                await TeamMemberRepo.GetLeadersAsync(
                    project.AssignedTeamId.Value,
                    ct);

            foreach (var leader in leaders)
            {
                var developerProfileId =
                    await UserRepo.GetDeveloperProfileIdByUserIdAsync(
                        leader.UserId,
                        ct);

                if (developerProfileId is null)
                    continue;

                BackgroundJob.Enqueue(() =>
                    _notificationService.CreateNotification(
                        new CreateNotificationRequest
                        {
                            DeveloperProfileId = developerProfileId.Value,
                            Title = "Milestone changes requested",
                            Body = notificationBody,
                            Type = NotificationType.MilestoneChangesRequested,
                            ProjectId = project.Id,
                            ActionUrl = $"/projects/{project.Id}?tab=milestones"
                        }));
            }
        }

        return ApiResponse.Success(string.IsNullOrWhiteSpace(comment)
            ? "Work changes requested."
            : $"Work changes requested: {comment.Trim()}");
    }

    public async Task<int> AutoReleaseDueMilestonesAsync(CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-MilestonePlanConstants.AutoReleaseDays);
        var due = (await _milestoneRepo.GetDueForAutoReleaseAsync(cutoff, ct)).ToList();
        var count = 0;

        foreach (var milestone in due)
        {
            var project = await _projectRepo.GetByIdAsync(milestone.ProjectId, ct);
            if (project is null) continue;

            try
            {
                await ReleaseFundsInternalAsync(project, milestone, ct);
                var actorId = project.AssignedUserId
                    ?? project.ClientId;
                await RecordEventAsync(
                    project.Id,
                    milestone.Id,
                    EventType.MilestoneReleased,
                    actorId,
                    ct);
                await TryCompleteProjectIfAllMilestonesReleasedAsync(project, milestone, ct);
                count++;
                if (project.AssignedUserId.HasValue)
                {
                    var developerProfileId =
                        await UserRepo.GetDeveloperProfileIdByUserIdAsync(
                            project.AssignedUserId.Value,
                            ct);

                    if (developerProfileId is not null)
                    {
                        BackgroundJob.Enqueue(() =>
                            _notificationService.CreateNotification(
                                new CreateNotificationRequest
                                {
                                    DeveloperProfileId = developerProfileId.Value,
                                    Title = "Milestone auto-released",
                                    Body = $"Milestone #{milestone.SortOrder} was automatically released after the review period.",
                                    Type = NotificationType.MilestoneReleased,
                                    ProjectId = project.Id,
                                    ActionUrl = $"/projects/{project.Id}?tab=milestones"
                                }));
                    }
                }
                else if (project.AssignedTeamId.HasValue)
                {
                    var leaders = await TeamMemberRepo.GetLeadersAsync(project.AssignedTeamId.Value, ct);
                    foreach (var leader in leaders)
                    {
                        var developerProfileId =
                            await UserRepo.GetDeveloperProfileIdByUserIdAsync(leader.UserId, ct);
                        if (developerProfileId is null)
                            continue;

                        BackgroundJob.Enqueue(() =>
                            _notificationService.CreateNotification(
                                new CreateNotificationRequest
                                {
                                    DeveloperProfileId = developerProfileId.Value,
                                    Title = "Milestone auto-released",
                                    Body = $"Milestone #{milestone.SortOrder} was automatically released after the review period.",
                                    Type = NotificationType.MilestoneReleased,
                                    ProjectId = project.Id,
                                    ActionUrl = $"/projects/{project.Id}?tab=milestones"
                                }));
                    }
                }
            }
            catch
            {
                // continue other milestones
            }
        }

        if (count > 0)
            await _unitOfWork.SaveChangesAsync(ct);

        count += await CompleteStuckReleasedProjectsAsync(ct);
        return count;
    }

    private async Task ReleaseFundsInternalAsync(
     Project project,
     Milestone milestone,
     CancellationToken ct)
    {
        var clientWallet =
            await WalletRepo.GetByOwnerAsync(
                owner.User,
                project.ClientId,
                ct)
            ?? throw new InvalidOperationException("Client wallet not found.");

        if (clientWallet.Reserved < milestone.Amount)
            throw new InvalidOperationException("Insufficient reserved funds.");

        var releaseKey = $"escrow-release:{milestone.Id}";

        if (await LedgerRepo.ExistsByIdempotencyKeyAsync(releaseKey, ct))
            return;

        clientWallet.Reserved -= milestone.Amount;

        if (project.AssignedUserId.HasValue)
        {
            var payeeWallet =
                await WalletRepo.GetByOwnerAsync(
                    owner.User,
                    project.AssignedUserId.Value,
                    ct)
                ?? throw new InvalidOperationException("Assignee wallet not found.");

            payeeWallet.Available += milestone.Amount;

            await LedgerRepo.AddAsync(new LedgerEntry
            {
                Id = Guid.NewGuid(),
                WalletId = payeeWallet.Id,
                EntryType = EntryType.EscrowRelease,
                Amount = milestone.Amount,
                Currency = payeeWallet.Currency,
                ProjectId = project.Id,
                MilestoneId = milestone.Id,
                IdempotencyKey = releaseKey
            }, ct);
        }
        else if (project.AssignedTeamId.HasValue)
        {
            await CreditTeamReleaseAsync(
                project,
                milestone,
                releaseKey,
                ct);
        }
        else
        {
            throw new InvalidOperationException("Project has no assignee.");
        }

        milestone.WorkStatus = WorkStatus.Approved;
        milestone.ReleaseStatus = ReleaseStatus.Released;
        milestone.ReleasedAmount = milestone.Amount;
        milestone.ReleasedAt = DateTime.UtcNow;

        await EscrowRepo.RecordReleaseAsync(
            project.Id,
            milestone.Amount,
            ct);
    }

    /// <summary>
    /// Team release: milestone splits → project override → team defaults → else 100% team wallet.
    /// </summary>
    private async Task CreditTeamReleaseAsync(
        Project project,
        Milestone milestone,
        string releaseKey,
        CancellationToken ct)
    {
        var teamId = project.AssignedTeamId!.Value;

        var splits = (await SplitRepo.GetByScopeAsync(teamId, project.Id, milestone.Id, ct)).ToList();
        if (splits.Count == 0)
            splits = (await SplitRepo.GetByScopeAsync(teamId, project.Id, null, ct)).ToList();
        if (splits.Count == 0)
            splits = (await SplitRepo.GetByScopeAsync(teamId, null, null, ct)).ToList();

        var useSplits = splits.Count > 0 &&
                        await SplitRepo.ValidateSplitsAsync(splits, milestone.Amount, allowPartialPercent: true, ct);

        if (!useSplits)
        {
            var teamWallet = await WalletRepo.GetByOwnerAsync(owner.Team, teamId, ct)
                ?? throw new InvalidOperationException("Team wallet not found.");

            teamWallet.Available += milestone.Amount;
            WalletRepo.Update(teamWallet);

            await LedgerRepo.AddAsync(new LedgerEntry
            {
                Id = Guid.NewGuid(),
                WalletId = teamWallet.Id,
                EntryType = EntryType.EscrowRelease,
                Amount = milestone.Amount,
                Currency = teamWallet.Currency,
                ProjectId = project.Id,
                MilestoneId = milestone.Id,
                IdempotencyKey = releaseKey
            }, ct);
            return;
        }

        decimal allocated = 0m;
        foreach (var split in splits)
        {
            decimal share = split.SplitType == SplitType.Percent
                ? Math.Round(milestone.Amount * split.Value / 100m, 2, MidpointRounding.AwayFromZero)
                : split.Value;

            if (share <= 0)
                continue;

            allocated += share;

            var memberWallet = await WalletRepo.GetByOwnerAsync(owner.User, split.UserId, ct)
                ?? throw new InvalidOperationException($"Wallet not found for team member {split.UserId}.");

            memberWallet.Available += share;
            WalletRepo.Update(memberWallet);

            await LedgerRepo.AddAsync(new LedgerEntry
            {
                Id = Guid.NewGuid(),
                WalletId = memberWallet.Id,
                EntryType = EntryType.TeamSplit,
                Amount = share,
                Currency = memberWallet.Currency,
                ProjectId = project.Id,
                MilestoneId = milestone.Id,
                IdempotencyKey = $"team-split:{milestone.Id}:{split.UserId}"
            }, ct);
        }

        var remainder = milestone.Amount - allocated;
        var teamWalletMarker = await WalletRepo.GetByOwnerAsync(owner.Team, teamId, ct)
            ?? throw new InvalidOperationException("Team wallet not found.");

        // Unallocated percent (or rounding leftover) stays on the team wallet.
        if (remainder > 0)
        {
            teamWalletMarker.Available += remainder;
            WalletRepo.Update(teamWalletMarker);

            await LedgerRepo.AddAsync(new LedgerEntry
            {
                Id = Guid.NewGuid(),
                WalletId = teamWalletMarker.Id,
                EntryType = EntryType.TeamSplit,
                Amount = remainder,
                Currency = teamWalletMarker.Currency,
                ProjectId = project.Id,
                MilestoneId = milestone.Id,
                IdempotencyKey = $"team-split-remainder:{milestone.Id}"
            }, ct);
        }

        // Parent release marker for idempotency / project audit.
        await LedgerRepo.AddAsync(new LedgerEntry
        {
            Id = Guid.NewGuid(),
            WalletId = teamWalletMarker.Id,
            EntryType = EntryType.EscrowRelease,
            Amount = milestone.Amount,
            Currency = teamWalletMarker.Currency,
            ProjectId = project.Id,
            MilestoneId = milestone.Id,
            IdempotencyKey = releaseKey
        }, ct);
    }

    private async Task<int> CompleteStuckReleasedProjectsAsync(CancellationToken ct)
    {
        var ids = await _projectRepo.GetInProgressIdsReadyToCompleteAsync(ct);
        if (ids.Count == 0)
            return 0;

        var completed = 0;
        foreach (var projectId in ids)
        {
            var project = await _projectRepo.GetByIdAsync(projectId, ct);
            if (project is null || project.Status == ProjectStatus.Completed)
                continue;

            try
            {
                await TryCompleteProjectIfAllMilestonesReleasedAsync(project, justReleased: null, ct);
                if (project.Status == ProjectStatus.Completed)
                    completed++;
            }
            catch
            {
                // continue other projects
            }
        }

        if (completed > 0)
            await _unitOfWork.SaveChangesAsync(ct);

        return completed;
    }

    /// <summary>
    /// Last released milestone: mark project Completed and archive the project chat.
    /// </summary>
    private async Task<(ChatRoom Room, Message Message)?> TryCompleteProjectIfAllMilestonesReleasedAsync(
        Project project,
        Milestone? justReleased,
        CancellationToken ct)
    {
        if (project.Status == ProjectStatus.Completed)
            return null;

        if (justReleased is not null)
        {
            var milestones = (await _milestoneRepo.GetByProjectIdAsync(project.Id, ct)).ToList();
            if (milestones.Count == 0)
                return null;

            var allReleased = milestones.All(m =>
                m.Id == justReleased.Id || m.ReleaseStatus == ReleaseStatus.Released);
            if (!allReleased)
                return null;
        }
        else if (!await _milestoneRepo.AllReleasedAsync(project.Id, ct))
        {
            return null;
        }

        project.Status = ProjectStatus.Completed;
        project.CompletedAt = DateTime.UtcNow;
        _projectRepo.Update(project);

        await RecordEventAsync(
            project.Id,
            justReleased?.Id,
            EventType.ProjectCompleted,
            project.ClientId,
            ct);

        var room = await ChatRoomRepo.GetByProjectIdForUpdateAsync(project.Id, ct);
        if (room is null || room.Status == ChatRoomStatus.Archived)
            return null;

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ChatRoomId = room.Id,
            MessageType = MessageType.System,
            Text = "Project completed. This conversation is now archived.",
            MilestoneId = justReleased?.Id,
            CreatedAt = DateTime.UtcNow
        };
        await MessageRepo.AddAsync(message, ct);

        room.Status = ChatRoomStatus.Archived;
        room.ArchivedAt = DateTime.UtcNow;
        room.UpdatedAt = DateTime.UtcNow;
        ChatRoomRepo.Update(room);

        return (room, message);
    }

    private async Task RecordEventAsync(
        Guid projectId,
        Guid? milestoneId,
        EventType type,
        Guid? actorUserId,
        CancellationToken ct)
    {
        await EventRepo.AddAsync(new ProjectEvent
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            MilestoneId = milestoneId,
            ActorUserId = actorUserId ?? _currentUser.UserId,
            EventType = type
        }, ct);
    }

    private IChatRoomRepository ChatRoomRepo =>
        _unitOfWork.Repository<IChatRoomRepository, ChatRoom>();

    private IMessageRepository MessageRepo =>
        _unitOfWork.Repository<IMessageRepository, Message>();

    private IChatRoomMemberRepository ChatMemberRepo =>
        _unitOfWork.Repository<IChatRoomMemberRepository, ChatRoomMember>();

    private IUserRepository UserRepo =>
        _unitOfWork.Repository<IUserRepository, User>();

    private async Task<(ChatRoom Room, Message Message)?> TryQueueProjectMilestoneChatAsync(
        Project project,
        Milestone milestone,
        MessageType type,
        string text,
        profileMode senderMode,
        CancellationToken ct)
    {
        var room = await ChatRoomRepo.GetByProjectIdAsync(project.Id, ct);
        if (room is null)
            return null;

        var senderProfiles = await ResolveSenderProfilesForModeAsync(senderMode, ct);
        if (senderProfiles is null)
            return null;

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ChatRoomId = room.Id,
            SenderClientProfileId = senderProfiles.Value.ClientProfileId,
            SenderDeveloperProfileId = senderProfiles.Value.DeveloperProfileId,
            MessageType = type,
            Text = text,
            MilestoneId = milestone.Id,
            CreatedAt = DateTime.UtcNow
        };
        await MessageRepo.AddAsync(message, ct);
        room.UpdatedAt = DateTime.UtcNow;
        return (room, message);
    }

    private async Task TryBroadcastQueuedChatAsync(
        (ChatRoom Room, Message Message)? queued,
        string preview,
        CancellationToken ct)
    {
        if (queued is null)
            return;
        try
        {
            await BroadcastChatMessageAsync(
                queued.Value.Room.Id,
                queued.Value.Message,
                preview,
                ct,
                queued.Value.Room.Status,
                queued.Value.Room.ArchivedAt);
        }
        catch
        {
            // Realtime is best-effort.
        }
    }

    private async Task BroadcastChatMessageAsync(
        Guid roomId,
        Message message,
        string? previewText,
        CancellationToken ct,
        ChatRoomStatus? roomStatus = null,
        DateTime? archivedAt = null)
    {
        var senderId = message.SenderClientProfileId ?? message.SenderDeveloperProfileId;
        var senderProfileType = message.SenderClientProfileId.HasValue
            ? nameof(profileMode.Client)
            : message.SenderDeveloperProfileId.HasValue
                ? nameof(profileMode.Developer)
                : null;
        var senderName = $"{_currentUser.FirstName} {_currentUser.LastName}".Trim();
        var dto = new RoomMessagesDto
        {
            Id = message.Id,
            ChatRoomId = roomId,
            SenderId = senderId,
            SenderProfileType = senderProfileType,
            SenderName = string.IsNullOrWhiteSpace(senderName) ? null : senderName,
            MessageType = message.MessageType.ToString(),
            Text = message.Text,
            FileName = message.FileName,
            FileUrl = message.FileUrl,
            PlanVersionId = message.PlanVersionId,
            MilestoneId = message.MilestoneId,
            CreatedAt = message.CreatedAt,
            IsMine = true
        };

        var roomUpdated = new RoomUpdatedDto
        {
            RoomId = roomId,
            LastMessage = previewText ?? message.Text ?? message.FileName,
            LastMessageType = message.MessageType.ToString(),
            LastMessageAt = message.CreatedAt,
            LastMessageSender = senderName,
            SenderId = senderId ?? Guid.Empty,
            Status = roomStatus?.ToString(),
            ArchivedAt = archivedAt
        };

        var profileIds = await ChatMemberRepo.GetRoomProfileIdsAsync(roomId);
        foreach (var profileId in profileIds)
        {
            await _hub.Clients.Group($"profile-{profileId}").SendAsync("ReceiveMessage", dto, ct);
            await _hub.Clients.Group($"profile-{profileId}").SendAsync("RoomUpdated", roomUpdated, ct);
        }
    }

    private async Task<AppError?> RequireActiveProfileModeAsync(profileMode required, CancellationToken ct)
    {
        var active = await UserRepo.GetActiveProfileAsync(_currentUser.UserId, ct);
        if (active is null)
        {
            return AppError.Validation(
                "An active profile is required. Create or switch to a Client or Developer profile.");
        }

        if (active.Value.Mode != required)
        {
            return AppError.Forbidden(required == profileMode.Client
                ? "Switch to Client profile to perform this action."
                : "Switch to Developer profile to perform this action.");
        }

        return null;
    }

    /// <summary>Stamp chat sender by action role (Client vs Developer), not by whatever mode happens to be active.</summary>
    private async Task<(Guid? ClientProfileId, Guid? DeveloperProfileId)?> ResolveSenderProfilesForModeAsync(
        profileMode required,
        CancellationToken ct)
    {
        if (required == profileMode.Client)
        {
            var clientProfileId = await UserRepo.GetClientProfileIdByUserIdAsync(_currentUser.UserId, ct);
            return clientProfileId is null ? null : (clientProfileId, null);
        }

        var developerProfileId = await UserRepo.GetDeveloperProfileIdByUserIdAsync(_currentUser.UserId, ct);
        return developerProfileId is null ? null : (null, developerProfileId);
    }

    private async Task<bool> CanAccessProjectMilestoneDataAsync(Project project, CancellationToken ct)
    {
        var userId = _currentUser.UserId;

        if (project.ClientId == userId)
            return true;

        if (project.AssignedUserId == userId)
            return true;

        if (project.AssignedTeamId is not null &&
            await TeamMemberRepo.IsMemberAsync(project.AssignedTeamId.Value, userId, ct))
            return true;

        // Pre-hire negotiation: discussion participants can read plans.
        var discussions = await ProposalRepo.GetActiveDiscussionByProjectIdAsync(project.Id, ct);
        foreach (var proposal in discussions)
        {
            if (proposal.ApplicantType == ApplicantType.User && proposal.UserId == userId)
                return true;

            if (proposal.ApplicantType == ApplicantType.Team &&
                proposal.TeamId is not null &&
                await TeamMemberRepo.IsMemberAsync(proposal.TeamId.Value, userId, ct))
                return true;
        }

        return false;
    }

    private async Task<bool> IsProposalNegotiationSpeakerAsync(ProjectProposal proposal, CancellationToken ct)
    {
        if (proposal.UserId != _currentUser.UserId)
            return false;

        if (proposal.ApplicantType == ApplicantType.User)
            return true;

        return proposal.TeamId is not null &&
               await TeamMemberRepo.IsLeaderAsync(proposal.TeamId.Value, _currentUser.UserId, ct);
    }

    private async Task<bool> IsProposalApplicantAsync(ProjectProposal proposal, CancellationToken ct)
    {
        if (proposal.ApplicantType == ApplicantType.User)
            return proposal.UserId == _currentUser.UserId;

        return proposal.TeamId is not null &&
               await TeamMemberRepo.IsLeaderAsync(proposal.TeamId.Value, _currentUser.UserId, ct);
    }

    private async Task<bool> IsProjectAssigneeAsync(Project project, CancellationToken ct)
    {
        if (project.AssignedUserId == _currentUser.UserId)
            return true;

        if (project.AssignedTeamId is not null)
            return await TeamMemberRepo.IsLeaderAsync(project.AssignedTeamId.Value, _currentUser.UserId, ct);

        return false;
    }

    private async Task NotifyProposalApplicantsAsync(
        ProjectProposal proposal,
        string title,
        string body,
        NotificationType type,
        Guid projectId,
        Guid proposalId,
        string actionUrl,
        CancellationToken ct)
    {
        if (proposal.ApplicantType == ApplicantType.User && proposal.UserId.HasValue)
        {
            var developerProfileId =
                await UserRepo.GetDeveloperProfileIdByUserIdAsync(proposal.UserId.Value, ct);
            if (developerProfileId is null)
                return;

            BackgroundJob.Enqueue(() =>
                _notificationService.CreateNotification(
                    new CreateNotificationRequest
                    {
                        DeveloperProfileId = developerProfileId.Value,
                        Title = title,
                        Body = body,
                        Type = type,
                        ProjectId = projectId,
                        ProjectProposalId = proposalId,
                        ActionUrl = actionUrl
                    }));
            return;
        }

        if (proposal.ApplicantType != ApplicantType.Team || !proposal.TeamId.HasValue)
            return;

        var leaders = await TeamMemberRepo.GetLeadersAsync(proposal.TeamId.Value, ct);
        var notified = new HashSet<Guid>();

        foreach (var leader in leaders)
        {
            var developerProfileId =
                await UserRepo.GetDeveloperProfileIdByUserIdAsync(leader.UserId, ct);
            if (developerProfileId is null || !notified.Add(developerProfileId.Value))
                continue;

            BackgroundJob.Enqueue(() =>
                _notificationService.CreateNotification(
                    new CreateNotificationRequest
                    {
                        DeveloperProfileId = developerProfileId.Value,
                        Title = title,
                        Body = body,
                        Type = type,
                        ProjectId = projectId,
                        ProjectProposalId = proposalId,
                        TeamId = proposal.TeamId,
                        ActionUrl = actionUrl
                    }));
        }

        if (proposal.UserId.HasValue)
        {
            var speakerProfileId =
                await UserRepo.GetDeveloperProfileIdByUserIdAsync(proposal.UserId.Value, ct);
            if (speakerProfileId is null || !notified.Add(speakerProfileId.Value))
                return;

            BackgroundJob.Enqueue(() =>
                _notificationService.CreateNotification(
                    new CreateNotificationRequest
                    {
                        DeveloperProfileId = speakerProfileId.Value,
                        Title = title,
                        Body = body,
                        Type = type,
                        ProjectId = projectId,
                        ProjectProposalId = proposalId,
                        TeamId = proposal.TeamId,
                        ActionUrl = actionUrl
                    }));
        }
    }

    /// <summary>
    /// Fixed price: plan total must equal the project budget.
    /// Range: plan total must stay within BudgetMin..BudgetMax.
    /// </summary>
    private static AppError? ValidatePlanTotalAgainstProjectBudget(Project project, decimal planTotal)
    {
        if (project.IsFixedPrice)
        {
            var fixedBudget = project.BudgetMax > 0 ? project.BudgetMax : project.BudgetMin;
            if (fixedBudget <= 0)
                return AppError.Validation("Project fixed budget is not configured.");

            if (planTotal != fixedBudget)
            {
                return AppError.Validation(
                    $"For a fixed-price project, milestone total must equal the project budget ({fixedBudget:0.##}). Current total: {planTotal:0.##}.");
            }

            return null;
        }

        if (project.BudgetMax < project.BudgetMin)
            return AppError.Validation("Project budget range is invalid.");

        if (planTotal < project.BudgetMin || planTotal > project.BudgetMax)
        {
            return AppError.Validation(
                $"Milestone total ({planTotal:0.##}) must be within the client budget range ({project.BudgetMin:0.##} – {project.BudgetMax:0.##}).");
        }

        return null;
    }

    private static MilestonePlanVersionDto MapPlanVersion(MilestonePlanVersion v) => new()
    {
        Id = v.Id,
        ProjectId = v.ProjectId,
        ProposalId = v.ProposalId,
        Version = v.Version,
        Status = v.Status.ToString(),
        ChangeComment = v.ChangeComment,
        ProposedByUserId = v.ProposedByUserId,
        CreatedAt = v.CreatedAt,
        Items = v.Items.OrderBy(i => i.SortOrder).Select(i => new MilestonePlanItemDto
        {
            Id = i.Id,
            Title = i.Title,
            DefinitionOfDone = i.DefinitionOfDone,
            Amount = i.Amount,
            DueDate = i.DueDate,
            SortOrder = i.SortOrder,
            ChangeTag = i.ChangeTag?.ToString()
        }).ToList()
    };
}
