using FreeGency.AI.HirePyInterview;
using FreeGency.AI.HirePyInterview.MilestonePlanning;
using FreeGency.Application.Common.Hubs;
using FreeGency.Application.Common.Interfaces;
using FreeGency.Application.Features.ChatFeature.Dtos;
using FreeGency.Application.Features.Milestones.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace FreeGency.Application.Features.HirePy;

/// <summary>
/// Private AI interviewer per accepted freelancer. The conversation lives in the existing
/// chat system (a dedicated room shared only by the freelancer and the HirePy AI bot);
/// this service manages the workflow state, the AI turns and the safe client signals.
/// </summary>
public sealed class HirePyInterviewService : IHirePyInterviewService
{
    internal const string BotEmail = "hirepy-ai@freegency.local";
    internal const string BotName = "HirePy AI";
    internal const string AiRoomTitle = "HirePy AI Interview";

    private const string BotPassword = "HirePy-AI!2026-Seeded";

    private static readonly TimeSpan StaleProcessingThreshold = TimeSpan.FromMinutes(2);
    private const int MaxFailureCount = 3;
    private const int ContextHistoryLimit = 10;
    private const int MaxPlanningRounds = 6;

    private static string? SafeText(Message message)
        => message.ModerationStatus == ModerationStatus.Redacted
           && !string.IsNullOrWhiteSpace(message.ModeratedText)
            ? message.ModeratedText
            : message.Text;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IHirePyInterviewAgent _agent;
    private readonly IHirePyMilestonePlannerAgent _plannerAgent;
    private readonly IMilestoneService _milestoneService;
    private readonly IHirePyEventPublisher _eventPublisher;
    private readonly INotificationService _notificationService;
    private readonly IHubContext<ChatHub> _hub;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IPasswordHasher<User> _passwordHasher;

    private readonly IHirePySessionRepository _sessionRepository;
    private readonly IHirePyInterviewRepository _interviewRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectProposalRepository _proposalRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDeveloperProfileRepository _developerProfileRepository;
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IChatRoomMemberRepository _chatRoomMemberRepository;
    private readonly IMessageRepository _messageRepository;

    public HirePyInterviewService(
        IUnitOfWork unitOfWork,
        IHirePyInterviewAgent agent,
        IHirePyMilestonePlannerAgent plannerAgent,
        IMilestoneService milestoneService,
        IHirePyEventPublisher eventPublisher,
        INotificationService notificationService,
        IHubContext<ChatHub> hub,
        IBackgroundJobClient backgroundJobClient,
        IPasswordHasher<User> passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _agent = agent;
        _plannerAgent = plannerAgent;
        _milestoneService = milestoneService;
        _eventPublisher = eventPublisher;
        _notificationService = notificationService;
        _hub = hub;
        _backgroundJobClient = backgroundJobClient;
        _passwordHasher = passwordHasher;

        _sessionRepository = unitOfWork.Repository<IHirePySessionRepository, HirePySession>();
        _interviewRepository = unitOfWork.Repository<IHirePyInterviewRepository, HirePyInterview>();
        _projectRepository = unitOfWork.Repository<IProjectRepository, Project>();
        _proposalRepository = unitOfWork.Repository<IProjectProposalRepository, ProjectProposal>();
        _userRepository = unitOfWork.Repository<IUserRepository, User>();
        _developerProfileRepository = unitOfWork.Repository<IDeveloperProfileRepository, DeveloperProfile>();
        _chatRoomRepository = unitOfWork.Repository<IChatRoomRepository, ChatRoom>();
        _chatRoomMemberRepository = unitOfWork.Repository<IChatRoomMemberRepository, ChatRoomMember>();
        _messageRepository = unitOfWork.Repository<IMessageRepository, Message>();
    }

    public async Task EnsureInterviewAsync(Guid sessionId, Guid proposalId, Guid freelancerUserId, CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, ct);
        if (session is null || session.ProjectId is null)
            return;

        var existing = await _interviewRepository.GetByProposalIdAsync(proposalId, ct);
        if (existing is not null)
            return;

        var proposal = await _proposalRepository.GetByIdAsync(proposalId, ct);
        if (proposal is null)
            return;

        var freelancerProfileId = await _userRepository.GetDeveloperProfileIdByUserIdAsync(freelancerUserId, ct);
        if (freelancerProfileId is null)
            return;

        var (botUserId, botProfileId) = await EnsureHirePyBotAsync(ct);

        var room = new ChatRoom
        {
            Id = Guid.NewGuid(),
            RoomType = RoomType.Proposal,
            Status = ChatRoomStatus.Active,
            Title = AiRoomTitle,
            ProjectId = null,
            ProposalId = null,
            CreatedByUserId = botUserId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = botUserId.ToString()
        };

        await _chatRoomRepository.AddWithMembersAsync(room,
        [
            (ClientProfileId: null, DeveloperProfileId: freelancerProfileId.Value, CanSend: true, RoleLabel: "Candidate"),
            (ClientProfileId: null, DeveloperProfileId: botProfileId, CanSend: true, RoleLabel: BotName)
        ], ct);

        var interview = new HirePyInterview
        {
            Id = Guid.NewGuid(),
            HirePySessionId = session.Id,
            ProjectId = session.ProjectId.Value,
            ProjectProposalId = proposal.Id,
            ChatRoomId = room.Id,
            FreelancerUserId = freelancerUserId,
            FreelancerDeveloperProfileId = freelancerProfileId.Value,
            Status = HirePyInterviewStatus.Started,
            TurnCount = 0,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };

        await _interviewRepository.AddAsync(interview, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _backgroundJobClient.Enqueue<IHirePyInterviewService>(
            s => s.ProcessInterviewAsync(interview.Id, CancellationToken.None));
    }

    public async Task ProcessInterviewAsync(Guid interviewId, CancellationToken ct = default)
    {
        var interview = await _interviewRepository.GetByIdAsync(interviewId, ct);
        if (interview is null || !IsActiveStatus(interview.Status))
            return;

        // Another worker may already be generating this turn.
        if (interview.ProcessingAt is { } processingAt
            && DateTime.UtcNow - processingAt < StaleProcessingThreshold)
            return;

        interview.ProcessingAt = DateTime.UtcNow;
        interview.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        try
        {
            await RunTurnAsync(interview, ct);
        }
        catch (Exception ex)
        {
            // Release the claim so the driver can retry; fail the interview only after
            // a bounded number of consecutive failures (survives transient AI errors).
            interview.ProcessingAt = null;
            interview.FailureCount++;
            interview.FailReason = ex.Message;
            interview.UpdatedAt = DateTime.UtcNow;
            if (interview.FailureCount >= MaxFailureCount)
            {
                interview.Status = HirePyInterviewStatus.Failed;
                interview.ConcludedAt = DateTime.UtcNow;
            }
            await _unitOfWork.SaveChangesAsync(ct);

            if (interview.Status == HirePyInterviewStatus.Failed)
                await PublishDiscussionFailedAsync(interview, ct);
        }
    }

    public async Task ProcessPendingAsync(CancellationToken ct = default)
    {
        var pending = await _interviewRepository.GetPendingAsync(ct);
        foreach (var interview in pending)
        {
            try
            {
                await ProcessInterviewAsync(interview.Id, ct);
            }
            catch
            {
                // Never let one interview break the whole poll cycle.
            }
        }
    }

    private async Task RunTurnAsync(HirePyInterview interview, CancellationToken ct)
    {
        var session = await _sessionRepository.GetByIdAsync(interview.HirePySessionId, ct);
        var project = await _projectRepository.GetByIdAsync(interview.ProjectId, ct);
        var proposal = await _proposalRepository.GetByIdAsync(interview.ProjectProposalId, ct);
        if (project is null || proposal is null)
            throw new InvalidOperationException("Project or proposal no longer exists.");

        var botProfileId = await GetBotProfileIdAsync(ct)
            ?? throw new InvalidOperationException("HirePy AI bot profile is not seeded.");

        var messages = await _messageRepository.Query()
            .Where(m => m.ChatRoomId == interview.ChatRoomId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);

        if (IsPlanningStatus(interview.Status))
        {
            await RunPlanningTurnAsync(
                interview, session, project, proposal, botProfileId, messages, ct);
            return;
        }

        var latestUserMessage = messages.LastOrDefault(m =>
            m.SenderDeveloperProfileId != botProfileId
            && m.ModerationStatus != ModerationStatus.Hidden);
        var isOpeningTurn = interview.TurnCount == 0 && interview.LastUserMessageId is null;

        if (!isOpeningTurn)
        {
            if (latestUserMessage is null || interview.LastUserMessageId == latestUserMessage.Id)
            {
                // Nothing new to answer — already handled this message.
                interview.ProcessingAt = null;
                await _unitOfWork.SaveChangesAsync(ct);
                return;
            }
        }

        var history = messages
            .Where(m => !string.IsNullOrWhiteSpace(SafeText(m)))
            .TakeLast(ContextHistoryLimit)
            .Select(m => m.SenderDeveloperProfileId == botProfileId
                ? (Role: BotName, Content: SafeText(m)!)
                : (Role: "Candidate", Content: SafeText(m)!))
            .ToList();

        var reply = await _agent.GetNextReplyAsync(new HirePyInterviewContext
        {
            CandidateName = await GetCandidateNameAsync(interview.FreelancerUserId, ct),
            ProjectBrief = BuildProjectBrief(project),
            ProposalSummary = BuildProposalSummary(proposal),
            History = history
        }, ct);

        if (string.IsNullOrWhiteSpace(reply.Message))
            throw new InvalidOperationException("The AI returned an empty reply.");

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ChatRoomId = interview.ChatRoomId,
            SenderDeveloperProfileId = botProfileId,
            MessageType = MessageType.Text,
            Text = reply.Message,
            ModerationStatus = ModerationStatus.Visible,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };
        await _messageRepository.AddAsync(message, ct);

        interview.TurnCount++;
        interview.LastUserMessageId = latestUserMessage?.Id;
        interview.FailureCount = 0;
        interview.ProcessingAt = null;
        if (reply.Decision == HirePyInterviewDecision.RequestMilestonePlan)
            interview.Status = HirePyInterviewStatus.MilestonePlanRequested;
        interview.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await BroadcastAiMessageAsync(interview, message, botProfileId);

        if (interview.TurnCount == 1 && interview.LastUserMessageId is null)
            await PublishDiscussionStartedAsync(session, project, ct);

        if (interview.Status == HirePyInterviewStatus.MilestonePlanRequested)
        {
            await PublishMilestonePlanRequestedAsync(session, project, ct);
            await PublishPlanningEventAsync(session, HirePyEventNames.MilestonePlanningStarted, project, ct);
        }
    }

    private async Task RunPlanningTurnAsync(
        HirePyInterview interview,
        HirePySession? session,
        Project project,
        ProjectProposal proposal,
        Guid botProfileId,
        IReadOnlyList<Message> messages,
        CancellationToken ct)
    {
        var latestUserMessage = messages.LastOrDefault(m =>
            m.SenderDeveloperProfileId != botProfileId
            && m.ModerationStatus != ModerationStatus.Hidden);
        if (latestUserMessage is null || interview.LastUserMessageId == latestUserMessage.Id)
        {
            // Nothing new to answer — already handled this plan message.
            interview.ProcessingAt = null;
            await _unitOfWork.SaveChangesAsync(ct);
            return;
        }

        interview.Status = HirePyInterviewStatus.MilestonePlanReceived;
        interview.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        await PublishPlanningEventAsync(session, HirePyEventNames.MilestonePlanReceived, project, ct);

        var history = messages
            .Where(m => !string.IsNullOrWhiteSpace(SafeText(m)))
            .TakeLast(ContextHistoryLimit)
            .Select(m => m.SenderDeveloperProfileId == botProfileId
                ? new MilestonePlanningHistoryEntry { Role = BotName, Content = SafeText(m)! }
                : new MilestonePlanningHistoryEntry { Role = "Candidate", Content = SafeText(m)! })
            .ToList();

        var isFinalAttempt = interview.PlanningRounds >= MaxPlanningRounds;
        var reply = await _plannerAgent.PlanNextStepAsync(new MilestonePlanningContext
        {
            CandidateName = await GetCandidateNameAsync(interview.FreelancerUserId, ct),
            ProjectBrief = BuildProjectBrief(project),
            ProposalSummary = BuildProposalSummary(proposal),
            History = history,
            IsFinalAttempt = isFinalAttempt
        }, ct);

        if (string.IsNullOrWhiteSpace(reply.Message))
            throw new InvalidOperationException("The AI returned an empty planning reply.");

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ChatRoomId = interview.ChatRoomId,
            SenderDeveloperProfileId = botProfileId,
            MessageType = MessageType.Text,
            Text = reply.Message,
            ModerationStatus = ModerationStatus.Visible,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };

        await PublishPlanningEventAsync(session, HirePyEventNames.MilestoneReviewStarted, project, ct);

        var planReady = reply.Outcome == MilestonePlanningOutcome.Finalize && reply.Milestones.Count > 0;
        string? failureNote = null;
        if (planReady)
        {
            var result = await _milestoneService.ProposePlanAsync(
                new ProposeMilestonePlanDto
                {
                    ProjectId = interview.ProjectId,
                    ProposalId = interview.ProjectProposalId,
                    Milestones = reply.Milestones.Select(m => new ProposeMilestoneItemDto
                    {
                        Title = m.Title,
                        DefinitionOfDone = m.DefinitionOfDone,
                        Amount = m.Amount,
                        DueDate = m.DueDate
                    }).ToList()
                },
                interview.FreelancerUserId,
                ct);

            if (result.IsSuccess)
            {
                interview.Status = HirePyInterviewStatus.MilestonePlanFinalized;
                interview.ConcludedAt = DateTime.UtcNow;
            }
            else
            {
                failureNote = result.Message;
                interview.Status = HirePyInterviewStatus.MilestoneRevisionRequested;
            }
        }
        else
        {
            interview.Status = HirePyInterviewStatus.MilestoneRevisionRequested;
        }

        if (!string.IsNullOrWhiteSpace(failureNote))
        {
            var note = failureNote.Trim();
            if (note.Length > 400)
                note = $"{note[..400].TrimEnd(' ', ',', '.')}…";
            message.Text = $"{message.Text}\n\nPlan note: {note}";
        }

        if (interview.Status == HirePyInterviewStatus.MilestoneRevisionRequested && isFinalAttempt)
        {
            interview.Status = HirePyInterviewStatus.Failed;
            interview.FailReason = string.IsNullOrWhiteSpace(failureNote)
                ? "The milestone plan could not be finalized within the allowed rounds."
                : failureNote;
            interview.ConcludedAt = DateTime.UtcNow;
        }

        await _messageRepository.AddAsync(message, ct);

        interview.TurnCount++;
        interview.PlanningRounds++;
        interview.LastUserMessageId = latestUserMessage.Id;
        interview.FailureCount = 0;
        interview.ProcessingAt = null;
        interview.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await BroadcastAiMessageAsync(interview, message, botProfileId);

        if (interview.Status == HirePyInterviewStatus.MilestonePlanFinalized)
            await PublishPlanFinalizedAsync(session, project, ct);
        else if (interview.Status == HirePyInterviewStatus.MilestoneRevisionRequested)
            await PublishPlanningEventAsync(session, HirePyEventNames.MilestoneRevisionRequested, project, ct);
        else if (interview.Status == HirePyInterviewStatus.Failed)
            await PublishDiscussionFailedAsync(interview, ct);
    }

    private static bool IsPlanningStatus(HirePyInterviewStatus status)
        => status is HirePyInterviewStatus.MilestonePlanRequested
            or HirePyInterviewStatus.MilestonePlanReceived
            or HirePyInterviewStatus.MilestoneRevisionRequested;

    private static bool IsActiveStatus(HirePyInterviewStatus status)
        => status == HirePyInterviewStatus.Started || IsPlanningStatus(status);

    private async Task BroadcastAiMessageAsync(
        HirePyInterview interview,
        Message message,
        Guid botProfileId)
    {
        var roomMembers = await _chatRoomMemberRepository.GetRoomProfileIdsAsync(interview.ChatRoomId);

        var recipientDto = new RoomMessagesDto
        {
            Id = message.Id,
            ChatRoomId = message.ChatRoomId,
            SenderId = botProfileId,
            SenderProfileType = profileMode.Developer.ToString(),
            SenderName = BotName,
            MessageType = MessageType.Text.ToString(),
            Text = message.Text,
            CreatedAt = message.CreatedAt,
            IsMine = false,
            ModerationStatus = ModerationStatus.Visible.ToString()
        };

        var roomUpdated = new RoomUpdatedDto
        {
            RoomId = message.ChatRoomId,
            LastMessage = message.Text,
            LastMessageType = MessageType.Text.ToString(),
            LastMessageAt = message.CreatedAt,
            LastMessageSender = BotName,
            SenderId = botProfileId
        };

        foreach (var memberRoom in roomMembers)
        {
            var profileId = memberRoom.ClientProfileId ?? memberRoom.DeveloperProfileId!.Value;
            if (profileId == botProfileId)
                continue;

            await _hub.Clients
                .Group($"profile-{profileId}")
                .SendAsync("ReceiveMessage", recipientDto);
            await _hub.Clients
                .Group($"profile-{profileId}")
                .SendAsync("RoomUpdated", roomUpdated);

            if (ChatHub.IsUserInRoom(interview.ChatRoomId, profileId))
                continue;

            _backgroundJobClient.Enqueue(() => _notificationService.CreateNotification(
                new CreateNotificationRequest
                {
                    Title = BotName,
                    Body = $"{BotName}: {message.Text}",
                    Type = NotificationType.NewChatMessage,
                    DeveloperProfileId = profileId,
                    ChatRoomId = interview.ChatRoomId,
                    MessageId = message.Id,
                    ActionUrl = $"/chat?room={interview.ChatRoomId}"
                }));
        }
    }

    private async Task PublishDiscussionStartedAsync(HirePySession? session, Project project, CancellationToken ct)
    {
        if (session is null)
            return;

        var stage = HirePySessionStatus.CandidateDiscussion.ToString();
        await _eventPublisher.PublishClientEventAsync(
            session.ClientUserId,
            HirePyEventNames.DiscussionStarted,
            new HirePyEventPayload(session.Id, "Processing", stage, project.Id),
            ct);

        var clientProfileId = await _userRepository.GetClientProfileIdByUserIdAsync(session.ClientUserId, ct);
        if (clientProfileId is null)
            return;

        await _notificationService.CreateNotification(new CreateNotificationRequest
        {
            ClientProfileId = clientProfileId,
            Title = "AI interview started",
            Body = "The HirePy AI has started a private technical discussion with one of your invited developers.",
            Type = NotificationType.HirePyDiscussionStarted,
            ProjectId = project.Id,
            ActionUrl = $"/client/projects/{project.Id}?tab=ai-interview",
            Data = System.Text.Json.JsonSerializer.Serialize(new { SessionId = session.Id })
        });
    }

    private async Task PublishMilestonePlanRequestedAsync(HirePySession? session, Project project, CancellationToken ct)
    {
        if (session is null)
            return;

        var stage = HirePySessionStatus.MilestonePlanning.ToString();
        await _eventPublisher.PublishClientEventAsync(
            session.ClientUserId,
            HirePyEventNames.MilestonePlanRequested,
            new HirePyEventPayload(session.Id, "Processing", stage, project.Id),
            ct);

        var clientProfileId = await _userRepository.GetClientProfileIdByUserIdAsync(session.ClientUserId, ct);
        if (clientProfileId is null)
            return;

        await _notificationService.CreateNotification(new CreateNotificationRequest
        {
            ClientProfileId = clientProfileId,
            Title = "Milestone plan requested",
            Body = "The AI interview wrapped up — the developer was asked for a milestone plan.",
            Type = NotificationType.HirePyMilestonePlanRequested,
            ProjectId = project.Id,
            ActionUrl = $"/client/projects/{project.Id}?tab=ai-interview",
            Data = System.Text.Json.JsonSerializer.Serialize(new { SessionId = session.Id })
        });
    }

    private async Task PublishPlanningEventAsync(
        HirePySession? session,
        string eventName,
        Project project,
        CancellationToken ct)
    {
        if (session is null)
            return;

        await _eventPublisher.PublishClientEventAsync(
            session.ClientUserId,
            eventName,
            new HirePyEventPayload(
                session.Id,
                "Processing",
                HirePySessionStatus.MilestonePlanning.ToString(),
                project.Id),
            ct);
    }

    private async Task PublishPlanFinalizedAsync(HirePySession? session, Project project, CancellationToken ct)
    {
        if (session is null)
            return;

        await PublishPlanningEventAsync(session, HirePyEventNames.MilestonePlanFinalized, project, ct);

        var clientProfileId = await _userRepository.GetClientProfileIdByUserIdAsync(session.ClientUserId, ct);
        if (clientProfileId is null)
            return;

        await _notificationService.CreateNotification(new CreateNotificationRequest
        {
            ClientProfileId = clientProfileId,
            Title = "Milestone plan finalized",
            Body = "The AI interview finalized a milestone plan for your project.",
            Type = NotificationType.HirePyMilestonePlanFinalized,
            ProjectId = project.Id,
            ActionUrl = $"/projects/{project.Id}?tab=milestones",
            Data = System.Text.Json.JsonSerializer.Serialize(new { SessionId = session.Id })
        });
    }

    private async Task PublishDiscussionFailedAsync(HirePyInterview interview, CancellationToken ct)
    {
        var session = await _sessionRepository.GetByIdAsync(interview.HirePySessionId, ct);
        if (session is null)
            return;

        await _eventPublisher.PublishClientEventAsync(
            session.ClientUserId,
            HirePyEventNames.DiscussionFailed,
            new HirePyEventPayload(
                session.Id,
                "Failed",
                HirePySessionStatus.Failed.ToString(),
                interview.ProjectId,
                Message: interview.FailReason),
            ct);
    }

    private async Task<(Guid UserId, Guid ProfileId)> EnsureHirePyBotAsync(CancellationToken ct)
    {
        var bot = await _developerProfileRepository.Query()
            .Where(dp => dp.IsHirePyBot && !dp.IsDeleted)
            .Select(dp => new { dp.Id, dp.UserId })
            .FirstOrDefaultAsync(ct);
        if (bot is not null)
            return (bot.UserId, bot.Id);

        var existingUser = await _userRepository.GetByEmailAsync(BotEmail, ct);
        var user = existingUser ?? new User
        {
            Id = Guid.NewGuid(),
            UserName = BotEmail,
            NormalizedUserName = BotEmail.ToUpperInvariant(),
            Email = BotEmail,
            NormalizedEmail = BotEmail.ToUpperInvariant(),
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            FristName = "HirePy",
            LastName = "AI",
            HasCompletedOnboarding = true,
            ActiveProfileMode = profileMode.Developer,
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };

        var userId = user.Id;
        if (existingUser is null)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, BotPassword);
            await _userRepository.AddAsync(user, ct);
        }

        var profile = new DeveloperProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            IsHirePyBot = true,
            Bio = "HirePy AI — FreeGency's AI interviewer.",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };
        await _developerProfileRepository.AddAsync(profile, ct);

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Concurrent seeding — a winner already exists.
            var winner = await _developerProfileRepository.Query()
                .Where(dp => dp.IsHirePyBot && !dp.IsDeleted)
                .Select(dp => new { dp.Id, dp.UserId })
                .FirstOrDefaultAsync(ct);
            if (winner is not null)
                return (winner.UserId, winner.Id);
            throw;
        }

        return (userId, profile.Id);
    }

    private async Task<Guid?> GetBotProfileIdAsync(CancellationToken ct)
    {
        return await _developerProfileRepository.Query()
            .Where(dp => dp.IsHirePyBot && !dp.IsDeleted)
            .Select(dp => (Guid?)dp.Id)
            .FirstOrDefaultAsync(ct);
    }

    private async Task<string> GetCandidateNameAsync(Guid userId, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user is null)
            return "the candidate";
        return $"{user.FristName} {user.LastName}".Trim();
    }

    private static string BuildProjectBrief(Project project)
        => $"Title: {project.Title}\n" +
           $"Description: {project.Description}\n" +
           $"Budget: {project.Currency} {project.BudgetMin} - {project.BudgetMax}\n" +
           $"Deadline: {project.Deadline:yyyy-MM-dd}\n" +
           $"Estimated duration: {project.EstimatedDurationDays} days";

    private static string BuildProposalSummary(ProjectProposal proposal)
        => $"Cover letter: {proposal.CoverLetter}\n" +
           $"Approach: {proposal.Approach}\n" +
           $"Proposed timeline: {proposal.ProposedTimeline}\n" +
           $"Proposed budget: {proposal.ProposedBudget}";
}
