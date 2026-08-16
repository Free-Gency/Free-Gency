
using FreeGency.Domain.Entities.TeamPlans;
using FreeGency.Domain.Interfaces.Repositories.plansTeam;
using FreeGency.Infrastructure.Persistence.Seeding;

namespace FreeGency.Application.Features.Teams.Commands;

public partial class TeamService : ITeamService
{
    private readonly ITeamRepository _teamRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageService _storageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IProjectRepository _projectRepository;
    private readonly IMilestonePlanVersionRepository _milestonePlanVersionRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly ILedgerEntryRepository _ledgerEntryRepository;
    private readonly IContentModerationService _contentModerationService;
    private readonly IUserRepository _userRepository;

    private readonly ITeamMemberRepository _teamMemberRepository;
    private readonly IMilestoneRepository _milestoneRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IMilestoneAssignmentRepository _milestoneAssignmentRepository;
    private readonly ITeamPayoutSplitRepository _payoutSplitRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ITeamPlanRepository teamPlanRepository;
    private readonly ITeamPlanFeatureRepository teamPlanFeatureRepository;
    private readonly ITeamSubscriptionRepository teamSubscriptionRepository;
    private readonly ITeamUsageRecordRepository teamUsageRecordRepository;
    public TeamService(IUnitOfWork unitOfWork, IStorageService storageService,
        ICurrentUserService currentUserService, IContentModerationService contentModerationService)
    {
        _unitOfWork = unitOfWork;
        _storageService = storageService;
        _currentUserService = currentUserService;
        _contentModerationService = contentModerationService;
        _walletRepository = _unitOfWork.Repository<IWalletRepository, Wallet>();
        _teamRepository = _unitOfWork.Repository<ITeamRepository, Team>();
        _projectRepository = _unitOfWork.Repository<IProjectRepository, Project>();
        _milestonePlanVersionRepository = _unitOfWork.Repository<IMilestonePlanVersionRepository, MilestonePlanVersion>();
        _ledgerEntryRepository = _unitOfWork.Repository<ILedgerEntryRepository, LedgerEntry>();
        _userRepository = _unitOfWork.Repository<IUserRepository, User>();
        _teamMemberRepository = _unitOfWork.Repository<ITeamMemberRepository, TeamMember>();

        _milestoneRepository = _unitOfWork.Repository<IMilestoneRepository, Milestone>();
        _projectMemberRepository = _unitOfWork.Repository<IProjectMemberRepository, ProjectMember>();
        _milestoneAssignmentRepository = _unitOfWork.Repository<IMilestoneAssignmentRepository, MilestoneAssignment>();
        _payoutSplitRepository = _unitOfWork.Repository<ITeamPayoutSplitRepository, TeamPayoutSplit>();
        _taskRepository = _unitOfWork.Repository<ITaskRepository, ProjectTask>();

        teamPlanRepository = _unitOfWork.Repository<ITeamPlanRepository, TeamPlan>();
        teamPlanFeatureRepository = _unitOfWork.Repository<ITeamPlanFeatureRepository, TeamPlanFeature>();
        teamSubscriptionRepository = _unitOfWork.Repository<ITeamSubscriptionRepository, TeamSubscription>();
        teamUsageRecordRepository = _unitOfWork.Repository<ITeamUsageRecordRepository, TeamUsageRecord>();
    }

    public async Task<ApiResponse<Guid>> CreateAsync(CreateTeamDto dto, CancellationToken ct = default)
    {
        string teamCode;
        do
        {
            teamCode = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        } while (await _teamRepository.TeamCodeExistsAsync(teamCode, ct));

        string? logoUrl = null;
        if (dto.Logo is not null)
        {
            try
            {
                logoUrl = (await _storageService.UploadAsync(dto.Logo, StorageFolders.TeamLogo, ct)).Url;
            }
            catch (Exception)
            {
                return ApiResponse.Failure<Guid>(AppError.FileUploadFailed(dto.Logo.FileName));
            }
        }

        string? coverUrl = null;
        if (dto.Cover is not null)
        {
            try
            {
                coverUrl = (await _storageService.UploadAsync(dto.Cover, StorageFolders.TeamLogo, ct)).Url;
            }
            catch (Exception)
            {
                return ApiResponse.Failure<Guid>(AppError.FileUploadFailed(dto.Cover.FileName));
            }
        }

        var ownerUserId = _currentUserService.UserId;
        var team = dto.ToEntity(ownerUserId, teamCode, logoUrl, coverUrl);

        var categories = dto.Categories.Select(c => (c.CategoryId, c.IsPrimary));
        await _teamRepository.AddWithTaxonomyAsync(team, categories, dto.SkillIds, ct);

        var memberRepo = _unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
        await memberRepo.AddAsync(new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = team.Id,
            UserId = ownerUserId,
            TeamRole = Role.TeamLeader,
            JoinedAt = DateTime.UtcNow.ToString("O")
        }, ct);

        var userRepo = _unitOfWork.Repository<IUserRepository, User>();
        var developerProfileId = await userRepo.GetDeveloperProfileIdByUserIdAsync(ownerUserId, ct);
        if (developerProfileId is null)
            return ApiResponse.Failure<Guid>(AppError.Validation(
                "A developer profile is required to create a team and its default chat."));

        var chatRoomRepo = _unitOfWork.Repository<IChatRoomRepository, ChatRoom>();
        var messageRepo = _unitOfWork.Repository<IMessageRepository, Message>();

        var mainRoom = new ChatRoom
        {
            Id = Guid.NewGuid(),
            RoomType = RoomType.TeamMain,
            Status = ChatRoomStatus.Active,
            TeamId = team.Id,
            Title = team.Name,
            CreatedByUserId = ownerUserId
        };

        await chatRoomRepo.AddWithMembersAsync(
            mainRoom,
            [(null, developerProfileId, true, "Team Leader")],
            ct);

        await messageRepo.AddAsync(new Message
        {
            Id = Guid.NewGuid(),
            ChatRoomId = mainRoom.Id,
            MessageType = MessageType.System,
            Text = $"Team chat created for {team.Name}. Leaders and members can message here."
        }, ct);
        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            OwnerType = owner.Team,
            OwnerTeamId = team.Id,
            Currency = "USD"
        };
        await _walletRepository.AddAsync(wallet);
        var now = DateTime.UtcNow;

        var freeSubscription = new TeamSubscription
        {
            Id = Guid.NewGuid(),

            TeamId = team.Id,

            TeamPlanId = TeamPlanSeeds.FreeTeamPlanId,

            BillingPeriod = BillingPeriod.Monthly,

            AutoRenew = false,

            StartedAt = now,

            ExpiresAt = now.AddMonths(1)
        };

        await teamSubscriptionRepository.AddAsync(freeSubscription, ct);

        var freePlanFeatures =
            await teamPlanFeatureRepository
                .GetByPlanIdAsync(TeamPlanSeeds.FreeTeamPlanId);

        foreach (var feature in freePlanFeatures.Where(x => x.IsEnabled))
        {
            await teamUsageRecordRepository.AddAsync(
                new TeamUsageRecord
                {
                    Id = Guid.NewGuid(),

                    TeamSubscriptionId = freeSubscription.Id,

                    Feature = feature.Feature,

                    Used = 0,

                    PeriodStart = now,

                    PeriodEnd = freeSubscription.ExpiresAt.Value
                },
                ct);
        }
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success(team.Id, "Team created successfully.");
    }

    public async Task<ApiResponse> UpdateAsync(UpdateTeamDto dto, CancellationToken ct = default)
    {
        var auth = await EnsureOwnerAsync(dto.Id, ct);
        if (!auth.IsSuccess)
            return ApiResponse.Failure(auth.Error!);

        var team = auth.Data!;
        team.Name = dto.Name;
        team.AboutUs = dto.AboutUs;

        if (dto.Logo is not null)
        {
            try
            {
                team.Logo = (await _storageService.UploadAsync(dto.Logo, StorageFolders.TeamLogo, ct)).Url;
            }
            catch (Exception)
            {
                return ApiResponse.Failure(AppError.FileUploadFailed(dto.Logo.FileName));
            }
        }

        if (dto.Cover is not null)
        {
            try
            {
                team.Cover = (await _storageService.UploadAsync(dto.Cover, StorageFolders.TeamLogo, ct)).Url;
            }
            catch (Exception)
            {
                return ApiResponse.Failure(AppError.FileUploadFailed(dto.Cover.FileName));
            }
        }

        _teamRepository.Update(team);
        await _unitOfWork.SaveChangesAsync(ct);
        BackgroundJob.Enqueue<ISuggestionService>(s => s.IndexTeamAsync(team.Id, CancellationToken.None));

        return ApiResponse.Success("Team updated successfully.");
    }

    public async Task<ApiResponse> ReplaceCategoriesAsync(Guid teamId, UpdateTeamCategoriesDto dto, CancellationToken ct = default)
    {
        var auth = await EnsureOwnerAsync(teamId, ct);
        if (!auth.IsSuccess)
            return ApiResponse.Failure(auth.Error!);

        var categories = dto.Categories.Select(c => (c.CategoryId, c.IsPrimary));
        await _teamRepository.ReplaceCategoriesAsync(teamId, categories, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        BackgroundJob.Enqueue<ISuggestionService>(s => s.IndexTeamAsync(teamId, CancellationToken.None));

        return ApiResponse.Success("Team categories updated successfully.");
    }

    public async Task<ApiResponse> ReplaceSpecialtiesAsync(Guid teamId, UpdateTeamSpecialtiesDto dto, CancellationToken ct = default)
    {
        var auth = await EnsureOwnerAsync(teamId, ct);
        if (!auth.IsSuccess)
            return ApiResponse.Failure(auth.Error!);

        await _teamRepository.ReplaceSpecialtiesAsync(teamId, dto.SpecialtyIds, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        BackgroundJob.Enqueue<ISuggestionService>(s => s.IndexTeamAsync(teamId, CancellationToken.None));

        return ApiResponse.Success("Team specialties updated successfully.");
    }

    public async Task<ApiResponse> ReplaceSkillsAsync(Guid teamId, UpdateTeamSkillsDto dto, CancellationToken ct = default)
    {
        var auth = await EnsureOwnerAsync(teamId, ct);
        if (!auth.IsSuccess)
            return ApiResponse.Failure(auth.Error!);

        await _teamRepository.ReplaceSkillsAsync(teamId, dto.SkillIds, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        BackgroundJob.Enqueue<ISuggestionService>(s => s.IndexTeamAsync(teamId, CancellationToken.None));

        return ApiResponse.Success("Team skills updated successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<TeamMemberDto>>> GetMembersAsync(
        Guid teamId,
        CancellationToken ct = default)
    {
        var team = await _teamRepository.GetByIdWithDetailsAsync(teamId, ct);
        if (team is null)
            return ApiResponse.Failure<IReadOnlyList<TeamMemberDto>>(AppError.NotFound(nameof(Team), teamId));

        var userId = _currentUserService.UserId;
        var isMember = team.OwnerUserId == userId
            || (team.TeamMembers?.Any(tm => tm.UserId == userId) ?? false);
        if (!isMember)
            return ApiResponse.Failure<IReadOnlyList<TeamMemberDto>>(
                AppError.Forbidden("Only team members can view the roster."));

        var memberRepo = _unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
        var members = await memberRepo.GetByTeamIdWithUserAsync(teamId, ct);

        var list = members.Select(tm => new TeamMemberDto
        {
            UserId = tm.UserId,
            Name = $"{tm.User?.FristName ?? string.Empty} {tm.User?.LastName ?? string.Empty}".Trim(),
            ImageUrl = tm.User?.DeveloperProfile?.ProfileImage
                ?? tm.User?.ClientProfile?.ProfileImage,
            Role = tm.TeamRole.ToString(),
            Job = tm.Job,
            IsOwner = tm.UserId == team.OwnerUserId,
            JoinedAt = tm.JoinedAt
        }).ToList();

        if (list.All(m => m.UserId != team.OwnerUserId) && team.OwnerUserId != Guid.Empty)
        {
            var ownerName = $"{team.Owner?.FristName ?? string.Empty} {team.Owner?.LastName ?? string.Empty}".Trim();
            list.Insert(0, new TeamMemberDto
            {
                UserId = team.OwnerUserId,
                Name = string.IsNullOrWhiteSpace(ownerName) ? "Owner" : ownerName,
                ImageUrl = team.Owner?.DeveloperProfile?.ProfileImage
                    ?? team.Owner?.ClientProfile?.ProfileImage,
                Role = nameof(Role.TeamLeader),
                IsOwner = true,
                JoinedAt = null
            });
        }

        return ApiResponse.Success<IReadOnlyList<TeamMemberDto>>(list);
    }

    public async Task<ApiResponse> UpdateMemberRoleAsync(
        Guid teamId,
        Guid userId,
        UpdateTeamMemberRoleDto dto,
        CancellationToken ct = default)
    {
        var team = await _teamRepository.GetByIdAsync(teamId, ct);
        if (team is null)
            return ApiResponse.Failure(AppError.NotFound(nameof(Team), teamId));

        if (!await IsLeaderOrOwnerAsync(team, _currentUserService.UserId, ct))
            return ApiResponse.Failure(AppError.Forbidden("Only team leaders can change member roles."));

        if (!Enum.TryParse<Role>(dto.Role, ignoreCase: true, out var newRole)
            || (newRole != Role.TeamLeader && newRole != Role.TeamMember))
            return ApiResponse.Failure(AppError.Validation("Role must be TeamLeader or TeamMember."));

        if (userId == team.OwnerUserId && newRole != Role.TeamLeader)
            return ApiResponse.Failure(AppError.Validation("The team owner must remain a Team Leader."));

        var memberRepo = _unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
        var member = await memberRepo.GetTrackedSingleInTeamAsync(teamId, userId, ct);
        if (member is null)
        {
            if (userId == team.OwnerUserId && newRole == Role.TeamLeader)
                return ApiResponse.Success("Owner is already a Team Leader.");
            return ApiResponse.Failure(AppError.NotFound(nameof(TeamMember), userId));
        }

        if (member.TeamRole == Role.TeamLeader && newRole == Role.TeamMember)
        {
            var leaders = await memberRepo.GetLeadersAsync(teamId, ct);
            var leaderCount = leaders.Count;
            if (team.OwnerUserId != Guid.Empty
                && leaders.All(l => l.UserId != team.OwnerUserId))
                leaderCount++;

            if (leaderCount <= 1)
                return ApiResponse.Failure(AppError.Validation("A team must keep at least one leader."));
        }

        member.TeamRole = newRole;
        memberRepo.Update(member);
        await _unitOfWork.SaveChangesAsync(ct);

        return ApiResponse.Success("Member role updated.");
    }

    public async Task<ApiResponse<Guid>> CreateTeamGroupAsync(
        Guid teamId,
        CreateTeamGroupDto dto,
        CancellationToken ct = default)
    {
        var title = (dto.Title ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(title))
            return ApiResponse.Failure<Guid>(AppError.Validation("Group title is required."));
        if (title.Length > 200)
            return ApiResponse.Failure<Guid>(AppError.Validation("Group title must be 200 characters or fewer."));

        var team = await _teamRepository.GetByIdAsync(teamId, ct);
        if (team is null)
            return ApiResponse.Failure<Guid>(AppError.NotFound(nameof(Team), teamId));

        var actorId = _currentUserService.UserId;
        if (!await IsLeaderOrOwnerAsync(team, actorId, ct))
            return ApiResponse.Failure<Guid>(AppError.Forbidden("Only team leaders can create private groups."));

        var memberRepo = _unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
        var teamMembers = await memberRepo.GetByTeamIdAsync(teamId, ct);
        var allowedUserIds = teamMembers.Select(m => m.UserId).ToHashSet();
        if (team.OwnerUserId != Guid.Empty)
            allowedUserIds.Add(team.OwnerUserId);

        if (!allowedUserIds.Contains(actorId))
            return ApiResponse.Failure<Guid>(AppError.Forbidden("You must belong to the team to create a group."));

        var inviteIds = (dto.MemberUserIds ?? [])
            .Where(id => id != Guid.Empty && allowedUserIds.Contains(id))
            .Append(actorId)
            .Distinct()
            .ToList();

        var userRepo = _unitOfWork.Repository<IUserRepository, User>();
        var chatMembers = new List<(Guid? ClientProfileId, Guid? DeveloperProfileId, bool CanSend, string? RoleLabel)>();

        foreach (var userId in inviteIds)
        {
            var developerProfileId = await userRepo.GetDeveloperProfileIdByUserIdAsync(userId, ct);
            if (developerProfileId is null)
                continue;

            var roleLabel = userId == actorId
                ? "Team Leader"
                : (teamMembers.FirstOrDefault(m => m.UserId == userId)?.TeamRole == Role.TeamLeader
                    || userId == team.OwnerUserId
                        ? "Team Leader"
                        : "Team Member");

            chatMembers.Add((null, developerProfileId, true, roleLabel));
        }

        if (chatMembers.Count == 0)
            return ApiResponse.Failure<Guid>(AppError.Validation(
                "At least one member with a developer profile is required."));

        var chatRoomRepo = _unitOfWork.Repository<IChatRoomRepository, ChatRoom>();
        var messageRepo = _unitOfWork.Repository<IMessageRepository, Message>();

        var room = new ChatRoom
        {
            Id = Guid.NewGuid(),
            RoomType = RoomType.TeamGroup,
            Status = ChatRoomStatus.Active,
            TeamId = teamId,
            Title = title,
            CreatedByUserId = actorId
        };

        await chatRoomRepo.AddWithMembersAsync(room, chatMembers, ct);
        await messageRepo.AddAsync(new Message
        {
            Id = Guid.NewGuid(),
            ChatRoomId = room.Id,
            MessageType = MessageType.System,
            Text = $"Private group “{title}” created."
        }, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        return ApiResponse.Success(room.Id, "Team group created.");
    }

    public async Task<ApiResponse> UpdateTeamChatRoomAsync(
        Guid teamId,
        Guid roomId,
        UpdateTeamChatRoomDto dto,
        CancellationToken ct = default)
    {
        var auth = await EnsureLeaderCanManageTeamRoomAsync(teamId, roomId, ct);
        if (!auth.IsSuccess)
            return ApiResponse.Failure(auth.Error!);

        var (_, room) = auth.Data!;
        var title = (dto.Title ?? string.Empty).Trim();
        var titleChanged = false;
        var logoChanged = false;

        if (!string.IsNullOrWhiteSpace(title))
        {
            if (title.Length > 200)
                return ApiResponse.Failure(AppError.Validation("Chat name must be 200 characters or fewer."));

            if (!string.Equals(room.Title, title, StringComparison.Ordinal))
            {
                room.Title = title;
                titleChanged = true;
            }
        }

        if (dto.Logo is not null)
        {
            try
            {
                room.Logo = (await _storageService.UploadAsync(dto.Logo, StorageFolders.ChatFiles, ct)).Url;
                logoChanged = true;
            }
            catch
            {
                return ApiResponse.Failure(AppError.FileUploadFailed(dto.Logo.FileName));
            }
        }

        if (!titleChanged && !logoChanged)
            return ApiResponse.Failure(AppError.Validation("Provide a new name and/or logo."));

        var chatRoomRepo = _unitOfWork.Repository<IChatRoomRepository, ChatRoom>();
        chatRoomRepo.Update(room);

        var messageRepo = _unitOfWork.Repository<IMessageRepository, Message>();
        var bits = new List<string>();
        if (titleChanged) bits.Add($"renamed to “{room.Title}”");
        if (logoChanged) bits.Add("updated the group photo");
        await messageRepo.AddAsync(new Message
        {
            Id = Guid.NewGuid(),
            ChatRoomId = room.Id,
            MessageType = MessageType.System,
            Text = $"Team leaders {string.Join(" and ", bits)}."
        }, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        return ApiResponse.Success("Chat room updated.");
    }

    public async Task<ApiResponse> AddTeamChatRoomMembersAsync(
        Guid teamId,
        Guid roomId,
        AddTeamChatRoomMembersDto dto,
        CancellationToken ct = default)
    {
        var auth = await EnsureLeaderCanManageTeamRoomAsync(teamId, roomId, ct);
        if (!auth.IsSuccess)
            return ApiResponse.Failure(auth.Error!);

        var (team, room) = auth.Data!;
        var memberRepo = _unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
        var teamMembers = await memberRepo.GetByTeamIdAsync(teamId, ct);
        var allowedUserIds = teamMembers.Select(m => m.UserId).ToHashSet();
        if (team.OwnerUserId != Guid.Empty)
            allowedUserIds.Add(team.OwnerUserId);

        var inviteIds = (dto.MemberUserIds ?? [])
            .Where(id => id != Guid.Empty && allowedUserIds.Contains(id))
            .Distinct()
            .ToList();

        if (inviteIds.Count == 0)
            return ApiResponse.Failure(AppError.Validation("Select at least one team member to add."));

        var userRepo = _unitOfWork.Repository<IUserRepository, User>();
        var chatRoomRepo = _unitOfWork.Repository<IChatRoomRepository, ChatRoom>();
        var addedNames = new List<string>();

        foreach (var userId in inviteIds)
        {
            var developerProfileId = await userRepo.GetDeveloperProfileIdByUserIdAsync(userId, ct);
            if (developerProfileId is null)
                continue;

            var roleLabel = userId == team.OwnerUserId
                || teamMembers.FirstOrDefault(m => m.UserId == userId)?.TeamRole == Role.TeamLeader
                    ? "Team Leader"
                    : "Team Member";

            var before = await _unitOfWork.Repository<IChatRoomMemberRepository, ChatRoomMember>()
                .IsMember(null, developerProfileId, roomId);
            if (before is not null)
                continue;

            await chatRoomRepo.AddMemberAsync(
                roomId,
                null,
                developerProfileId,
                ct,
                canSend: true,
                roleLabel: roleLabel);

            var user = await userRepo.GetByIdAsync(userId, ct);
            var name = user is null
                ? "Member"
                : $"{user.FristName} {user.LastName}".Trim();
            if (!string.IsNullOrWhiteSpace(name))
                addedNames.Add(name);
        }

        if (addedNames.Count == 0)
            return ApiResponse.Failure(AppError.Validation("Selected members are already in this chat."));

        var messageRepo = _unitOfWork.Repository<IMessageRepository, Message>();
        await messageRepo.AddAsync(new Message
        {
            Id = Guid.NewGuid(),
            ChatRoomId = room.Id,
            MessageType = MessageType.System,
            Text = addedNames.Count == 1
                ? $"{addedNames[0]} was added to the group."
                : $"{string.Join(", ", addedNames)} were added to the group."
        }, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        return ApiResponse.Success("Members added to chat.");
    }

    public async Task<ApiResponse<IReadOnlyList<TeamChatRoomMemberDto>>> GetTeamChatRoomMembersAsync(
        Guid teamId,
        Guid roomId,
        CancellationToken ct = default)
    {
        var auth = await EnsureLeaderCanManageTeamRoomAsync(teamId, roomId, ct);
        if (!auth.IsSuccess)
            return ApiResponse.Failure<IReadOnlyList<TeamChatRoomMemberDto>>(auth.Error!);

        var memberRepo = _unitOfWork.Repository<IChatRoomMemberRepository, ChatRoomMember>();
        var rows = await memberRepo.GetDeveloperMembersAsync(roomId, ct);
        var dto = rows
            .Select(r => new TeamChatRoomMemberDto
            {
                UserId = r.UserId,
                Name = string.IsNullOrWhiteSpace(r.Name) ? "Member" : r.Name,
                RoleLabel = r.RoleLabel,
                CanSend = r.CanSend
            })
            .OrderBy(r => r.Name)
            .ToList();

        return ApiResponse.Success<IReadOnlyList<TeamChatRoomMemberDto>>(dto);
    }

    private async Task<ApiResponse<Team>> EnsureOwnerAsync(Guid teamId, CancellationToken ct)
    {
        var team = await _teamRepository.GetByIdAsync(teamId, ct);
        if (team is null)
            return ApiResponse.Failure<Team>(AppError.NotFound(nameof(Team), teamId));

        if (team.OwnerUserId != _currentUserService.UserId)
            return ApiResponse.Failure<Team>(
                AppError.Forbidden("You are not authorized to perform this action on this team."));

        return ApiResponse.Success(team);
    }

    private async Task<bool> IsLeaderOrOwnerAsync(Team team, Guid userId, CancellationToken ct)
    {
        if (team.OwnerUserId == userId)
            return true;

        var memberRepo = _unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
        return await memberRepo.IsLeaderAsync(team.Id, userId, ct);
    }

    private async Task<ApiResponse<(Team Team, ChatRoom Room)>> EnsureLeaderCanManageTeamRoomAsync(
        Guid teamId,
        Guid roomId,
        CancellationToken ct)
    {
        var team = await _teamRepository.GetByIdAsync(teamId, ct);
        if (team is null)
            return ApiResponse.Failure<(Team, ChatRoom)>(AppError.NotFound(nameof(Team), teamId));

        var actorId = _currentUserService.UserId;
        if (!await IsLeaderOrOwnerAsync(team, actorId, ct))
            return ApiResponse.Failure<(Team, ChatRoom)>(
                AppError.Forbidden("Only team leaders can manage team chats."));

        var chatRoomRepo = _unitOfWork.Repository<IChatRoomRepository, ChatRoom>();
        var room = await chatRoomRepo.GetByIdAsync(roomId, ct);
        if (room is null || room.TeamId != teamId)
            return ApiResponse.Failure<(Team, ChatRoom)>(AppError.NotFound(nameof(ChatRoom), roomId));

        if (room.RoomType is not (RoomType.TeamMain or RoomType.TeamGroup))
            return ApiResponse.Failure<(Team, ChatRoom)>(
                AppError.Validation("Only team main/group chats can be managed here."));

        return ApiResponse.Success((team, room));
    }

    public async Task<ApiResponse<TeamReviewDto>> AddReviewAsync(
        Guid teamId,
        CreateTeamFeedbackRequestDto request,
        CancellationToken ct = default)
    {
        var team = await _teamRepository.GetByIdAsync(teamId, ct);
        if (team is null)
            return ApiResponse.Failure<TeamReviewDto>(AppError.NotFound(nameof(Team), teamId));

        var userId = _currentUserService.UserId;
        if (userId == Guid.Empty)
            return ApiResponse.Failure<TeamReviewDto>(AppError.Unauthorized());

        var memberRepo = _unitOfWork.Repository<ITeamMemberRepository, TeamMember>();
        if (team.OwnerUserId == userId || await memberRepo.IsMemberAsync(teamId, userId, ct))
            return ApiResponse.Failure<TeamReviewDto>(
                AppError.Forbidden("You cannot review a team you belong to."));

        if (request.Rating is < 1 or > 5)
            return ApiResponse.Failure<TeamReviewDto>(
                AppError.Validation("Rating must be between 1 and 5."));

        var comment = string.IsNullOrWhiteSpace(request.Comment)
            ? null
            : request.Comment.Trim();

        if (comment is { Length: > 500 })
            return ApiResponse.Failure<TeamReviewDto>(
                AppError.Validation("Comment must be 500 characters or fewer."));

        if (await _teamRepository.HasFeedbackAsync(teamId, userId, ct))
            return ApiResponse.Failure<TeamReviewDto>(
                AppError.Conflict("You already reviewed this team."));

        var (isMuted, mutedUntil) = await _contentModerationService.GetMuteStatusAsync(userId, ct);
        if (isMuted)
            return ApiResponse.Failure<TeamReviewDto>(
                AppError.Forbidden($"You are temporarily restricted from posting reviews until {mutedUntil:u}."));

        var now = DateTime.UtcNow;
        var feedback = new TeamFeedback
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            ReviewerUserId = userId,
            Rating = request.Rating,
            Comment = comment,
            CreatedAt = now,
            CreatedBy = userId.ToString(),
            ModerationStatus = ModerationStatus.Visible
        };

        await _teamRepository.AddFeedbackAsync(feedback, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        string? moderationWarning = null;
        if (!string.IsNullOrWhiteSpace(comment))
        {
            var active = await _userRepository.GetActiveProfileAsync(userId, ct);
            Guid? clientProfileId = null;
            Guid? developerProfileId = null;
            if (active is not null)
            {
                if (active.Value.Mode == profileMode.Client) clientProfileId = active.Value.ProfileId;
                else developerProfileId = active.Value.ProfileId;
            }

            var moderation = await _contentModerationService.ModerateAndEnforceAsync(
                userId,
                ModerationSourceType.TeamFeedback,
                feedback.Id,
                comment,
                "review",
                clientProfileId,
                developerProfileId,
                ct);

            feedback.ModerationStatus = moderation.Status;
            feedback.ModerationNote = moderation.WarningMessage;
            feedback.ModeratedText = moderation.Status switch
            {
                ModerationStatus.Visible => null,
                ModerationStatus.Redacted => moderation.SafeText,
                _ => "Review comment removed by FreeGency for a policy violation."
            };
            moderationWarning = moderation.WarningMessage;
            await _unitOfWork.SaveChangesAsync(ct);

            if (moderation.Action == ModerationAction.BlockSubmit)
                return ApiResponse.Failure<TeamReviewDto>(
                    AppError.Validation(moderation.WarningMessage ?? "This review violates FreeGency community guidelines."));
        }

        var all = await _teamRepository.GetFeedbackAsync(teamId, 500, ct);
        var count = all.Count;
        var average = count == 0 ? 0m : (decimal)all.Average(x => x.Rating);
        await _teamRepository.UpdateRatingAsync(teamId, Math.Round(average, 2), count, ct);

        var withUser = all.FirstOrDefault(x => x.Id == feedback.Id);
        var dto = MapTeamReview(withUser ?? feedback);
        dto.ModerationWarning = moderationWarning;
        return ApiResponse.Success(dto);
    }

    private static TeamReviewDto MapTeamReview(TeamFeedback feedback)
    {
        var user = feedback.ReviewerUser;
        var name = user is null
            ? string.Empty
            : $"{user.FristName} {user.LastName}".Trim();

        if (string.IsNullOrWhiteSpace(name))
            name = user?.UserName?.Trim() ?? "Community member";

        var comment = feedback.ModerationStatus switch
        {
            ModerationStatus.Visible => feedback.Comment,
            ModerationStatus.Redacted => feedback.ModeratedText ?? feedback.Comment,
            _ => feedback.ModeratedText ?? "Review comment removed by FreeGency for a policy violation."
        };

        return new TeamReviewDto
        {
            Id = feedback.Id,
            Rating = feedback.Rating,
            Comment = comment,
            CreatedAt = feedback.CreatedAt,
            ReviewerUserId = feedback.ReviewerUserId,
            ReviewerName = name,
            ReviewerAvatar = user?.DeveloperProfile?.ProfileImage
                ?? user?.ClientProfile?.ProfileImage,
            ModerationStatus = feedback.ModerationStatus.ToString(),
        };
    }

  
}