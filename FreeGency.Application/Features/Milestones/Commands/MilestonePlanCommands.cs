using FreeGency.Application.Features.Milestones.DTOs;
using FreeGency.Domain.Constants;
using FreeGency.Domain.Interfaces.Repositories.Teams;

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

    public async Task<ApiResponse<IEnumerable<MilestonePlanVersionDto>>> GetPlanVersionsAsync(
        Guid projectId, CancellationToken ct = default)
    {
        var project = await _projectRepo.GetByIdAsync(projectId, ct);
        if (project is null)
            return ApiResponse.Failure<IEnumerable<MilestonePlanVersionDto>>(AppError.NotFound(nameof(Project), projectId));

        var versions = await PlanRepo.GetByProjectIdAsync(projectId, ct);
        return ApiResponse.Success(versions.Select(MapPlanVersion));
    }

    public async Task<ApiResponse<MilestonePlanVersionDto>> GetLatestPlanAsync(
        Guid projectId, CancellationToken ct = default)
    {
        var project = await _projectRepo.GetByIdAsync(projectId, ct);
        if (project is null)
            return ApiResponse.Failure<MilestonePlanVersionDto>(AppError.NotFound(nameof(Project), projectId));

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

        if (project.AssignedUserId is not null || project.AssignedTeamId is not null)
            return ApiResponse.Failure<MilestonePlanVersionDto>(AppError.Validation("Project already has a hired assignee."));

        if (!await IsProposalNegotiationSpeakerAsync(proposal, ct))
            return ApiResponse.Failure<MilestonePlanVersionDto>(AppError.Forbidden(
                "Only the team leader who submitted this proposal can propose a milestone plan."));

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
                                      || !string.Equals(p.DefinitionOfDone, m.DefinitionOfDone, StringComparison.Ordinal)
                                      || p.Amount != m.Amount
                                      || p.DueDate != m.DueDate;
                        if (changed) tag = MilestoneChangeTag.Updated;
                    }
                }

                return new MilestonePlanItem
                {
                    Id = Guid.NewGuid(),
                    Title = m.Title.Trim(),
                    DefinitionOfDone = m.DefinitionOfDone.Trim(),
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

        var proposalRoom = await ChatRoomRepo.GetByProposalIdAsync(dto.ProposalId, ct);
        if (proposalRoom is not null)
        {
            var senderProfiles = await ResolveActiveSenderProfilesAsync(ct);
            if (senderProfiles is null)
                return ApiResponse.Failure<MilestonePlanVersionDto>(
                    AppError.Validation("An active client or developer profile is required for chat."));

            await MessageRepo.AddAsync(new Message
            {
                Id = Guid.NewGuid(),
                ChatRoomId = proposalRoom.Id,
                SenderClientProfileId = senderProfiles.Value.ClientProfileId,
                SenderDeveloperProfileId = senderProfiles.Value.DeveloperProfileId,
                MessageType = MessageType.MilestonePlan,
                Text = $"Milestone Plan v{nextVersion} proposed.",
                PlanVersionId = plan.Id
            }, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        var saved = await PlanRepo.GetByIdWithItemsAsync(plan.Id, ct);
        return ApiResponse.Success(MapPlanVersion(saved!), $"Milestone plan v{nextVersion} proposed.");
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

        if (plan.Status != PlanVersionStatus.Proposed)
            return ApiResponse.Failure(AppError.Validation("Only a proposed plan can receive change requests."));

        if (string.IsNullOrWhiteSpace(dto.Comment))
            return ApiResponse.Failure(AppError.Validation("A general comment is required."));

        plan.Status = PlanVersionStatus.ChangesRequested;
        plan.ChangeComment = dto.Comment.Trim();
        PlanRepo.Update(plan);

        await EscrowRepo.UpdatePlanStatusAsync(plan.ProjectId, PlanStatus.PlanRevisionRequested, ct);

        var proposalRoom = await ChatRoomRepo.GetByProposalIdAsync(plan.ProposalId, ct);
        if (proposalRoom is not null)
        {
            var senderProfiles = await ResolveActiveSenderProfilesAsync(ct);
            if (senderProfiles is null)
                return ApiResponse.Failure(
                    AppError.Validation("An active client or developer profile is required for chat."));

            await MessageRepo.AddAsync(new Message
            {
                Id = Guid.NewGuid(),
                ChatRoomId = proposalRoom.Id,
                SenderClientProfileId = senderProfiles.Value.ClientProfileId,
                SenderDeveloperProfileId = senderProfiles.Value.DeveloperProfileId,
                MessageType = MessageType.Text,
                Text = $"Request Changes on plan v{plan.Version}: {plan.ChangeComment}"
            }, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

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

        if (plan.Status != PlanVersionStatus.Proposed)
            return ApiResponse.Failure(AppError.Validation("Only a proposed plan can be accepted."));

        var proposal = await ProposalRepo.GetByIdAsync(plan.ProposalId, ct);
        if (proposal is null || proposal.Status != ProposalStatus.InDiscussion)
            return ApiResponse.Failure(AppError.Validation("Linked proposal must be In Discussion."));

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

        await _projectRepo.SetAssigneeAsync(
            plan.ProjectId,
            proposal.ApplicantType == ApplicantType.User ? proposal.UserId : null,
            proposal.ApplicantType == ApplicantType.Team ? proposal.TeamId : null,
            ct);

        await EscrowRepo.UpdatePlanStatusAsync(plan.ProjectId, PlanStatus.PlanAgreed, ct);

        // Reject cascade — only at Hire
        var others = await ProposalRepo.GetCascadeRejectCandidatesAsync(plan.ProjectId, proposal.Id, ct);
        foreach (var other in others)
        {
            await ProposalRepo.UpdateStatusAsync(
                other.Id,
                ProposalStatus.Rejected,
                MilestonePlanConstants.HiredAnotherCandidateReason,
                ct);
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
                Text = $"Project started — {project.Title}. Milestone plan agreed. Team leaders can add working members to this room."
            }, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return ApiResponse.Success("Milestone plan accepted — hire complete. Fund Milestone #1 to start work.");
    }

    public async Task<ApiResponse> FundNextMilestoneAsync(Guid projectId, CancellationToken ct = default)
    {
        var project = await _projectRepo.GetByIdAsync(projectId, ct);
        if (project is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Project), projectId));

        if (project.ClientId != _currentUser.UserId)
            return ApiResponse.Failure(AppError.Forbidden("Only the client can fund milestones."));

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

        await _unitOfWork.SaveChangesAsync(ct);
        return ApiResponse.Success($"Milestone #{next.SortOrder} funded in escrow (${next.Amount}).");
    }

    public async Task<ApiResponse> SubmitMilestoneAsync(Guid milestoneId, CancellationToken ct = default)
    {
        var milestone = await _milestoneRepo.GetByIdAsync(milestoneId, ct);
        if (milestone is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Milestone), milestoneId));

        var project = await _projectRepo.GetByIdAsync(milestone.ProjectId, ct);
        if (project is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Project), milestone.ProjectId));

        if (!await IsProjectAssigneeAsync(project, ct))
            return ApiResponse.Failure(AppError.Forbidden("Only the hired assignee can submit a milestone."));

        if (!milestone.IsFunded)
            return ApiResponse.Failure(AppError.Validation("Milestone must be funded before submission."));

        if (milestone.WorkStatus is not (WorkStatus.InProgress or WorkStatus.ChangesRequested))
            return ApiResponse.Failure(AppError.Validation("Milestone is not ready for submission."));

        milestone.WorkStatus = WorkStatus.Submitted;
        milestone.SubmittedAt = DateTime.UtcNow;
        milestone.ReleaseStatus = ReleaseStatus.Pending;
        milestone.AvailableAt = DateTime.UtcNow;
        _milestoneRepo.Update(milestone);

        await _unitOfWork.SaveChangesAsync(ct);
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

        if (milestone.WorkStatus != WorkStatus.Submitted || milestone.ReleaseStatus != ReleaseStatus.Pending)
            return ApiResponse.Failure(AppError.Validation("Milestone is not awaiting approval."));

        await ReleaseFundsInternalAsync(project, milestone, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return ApiResponse.Success("Funds released from escrow to the assignee.");
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

        if (milestone.WorkStatus != WorkStatus.Submitted)
            return ApiResponse.Failure(AppError.Validation("Only submitted milestones can receive change requests."));

        milestone.WorkStatus = WorkStatus.ChangesRequested;
        milestone.ReleaseStatus = ReleaseStatus.Locked;
        milestone.AvailableAt = null;
        _milestoneRepo.Update(milestone);
        await _unitOfWork.SaveChangesAsync(ct);

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
                count++;
            }
            catch
            {
                // continue other milestones
            }
        }

        if (count > 0)
            await _unitOfWork.SaveChangesAsync(ct);

        return count;
    }

    private async Task ReleaseFundsInternalAsync(Project project, Milestone milestone, CancellationToken ct)
    {
        var clientWallet = await WalletRepo.GetByOwnerAsync(owner.User, project.ClientId, ct)
            ?? throw new InvalidOperationException("Client wallet not found.");

        if (clientWallet.Reserved < milestone.Amount)
            throw new InvalidOperationException("Insufficient reserved funds.");

        Wallet? payeeWallet = null;
        if (project.AssignedUserId.HasValue)
            payeeWallet = await WalletRepo.GetByOwnerAsync(owner.User, project.AssignedUserId.Value, ct);
        else if (project.AssignedTeamId.HasValue)
            payeeWallet = await WalletRepo.GetByOwnerAsync(owner.Team, project.AssignedTeamId.Value, ct);

        if (payeeWallet is null)
            throw new InvalidOperationException("Assignee wallet not found.");

        var releaseKey = $"escrow-release:{milestone.Id}";
        if (await LedgerRepo.ExistsByIdempotencyKeyAsync(releaseKey, ct))
            return;

        clientWallet.Reserved -= milestone.Amount;
        WalletRepo.Update(clientWallet);

        payeeWallet.Available += milestone.Amount;
        WalletRepo.Update(payeeWallet);

        milestone.WorkStatus = WorkStatus.Approved;
        milestone.ReleaseStatus = ReleaseStatus.Released;
        milestone.ReleasedAmount = milestone.Amount;
        milestone.ReleasedAt = DateTime.UtcNow;
        _milestoneRepo.Update(milestone);

        await EscrowRepo.RecordReleaseAsync(project.Id, milestone.Amount, ct);

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

    private IChatRoomRepository ChatRoomRepo =>
        _unitOfWork.Repository<IChatRoomRepository, ChatRoom>();

    private IMessageRepository MessageRepo =>
        _unitOfWork.Repository<IMessageRepository, Message>();

    private IUserRepository UserRepo =>
        _unitOfWork.Repository<IUserRepository, User>();

    private async Task<(Guid? ClientProfileId, Guid? DeveloperProfileId)?> ResolveActiveSenderProfilesAsync(
        CancellationToken ct)
    {
        var active = await UserRepo.GetActiveProfileAsync(_currentUser.UserId, ct);
        if (active is null)
            return null;

        return active.Value.Mode == profileMode.Client
            ? (active.Value.ProfileId, null)
            : (null, active.Value.ProfileId);
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
