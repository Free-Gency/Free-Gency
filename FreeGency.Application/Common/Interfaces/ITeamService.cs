

namespace FreeGency.Application.Common.Interfaces;

public interface ITeamService
{
    Task<Result<WalletTeamDto>> GetTeamWallet(Guid teamid);
    Task<ApiResponse<PaginatedResult<TeamDto>>> BrowseAsync(FilterTeamsRequestDto filter, CancellationToken ct = default);
    Task<ApiResponse<TeamDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<TeamDto>>> GetMineAsync(CancellationToken ct = default);
    Task<ApiResponse<TeamDto>> GetByTeamCodeAsync(string teamCode, CancellationToken ct = default);
    Task<ApiResponse<Guid>> CreateAsync(CreateTeamDto dto, CancellationToken ct = default);
    Task<ApiResponse> UpdateAsync(UpdateTeamDto dto, CancellationToken ct = default);
    Task<ApiResponse> ReplaceCategoriesAsync(Guid teamId, UpdateTeamCategoriesDto dto, CancellationToken ct = default);
    Task<ApiResponse> ReplaceSpecialtiesAsync(Guid teamId, UpdateTeamSpecialtiesDto dto, CancellationToken ct = default);
    Task<ApiResponse> ReplaceSkillsAsync(Guid teamId, UpdateTeamSkillsDto dto, CancellationToken ct = default);
    Task<ApiResponse<IReadOnlyList<TeamMemberDto>>> GetMembersAsync(Guid teamId, CancellationToken ct = default);
    Task<ApiResponse> UpdateMemberRoleAsync(Guid teamId, Guid userId, UpdateTeamMemberRoleDto dto, CancellationToken ct = default);
    Task<ApiResponse<Guid>> CreateTeamGroupAsync(Guid teamId, CreateTeamGroupDto dto, CancellationToken ct = default);
    Task<ApiResponse> UpdateTeamChatRoomAsync(Guid teamId, Guid roomId, UpdateTeamChatRoomDto dto, CancellationToken ct = default);
    Task<ApiResponse> AddTeamChatRoomMembersAsync(Guid teamId, Guid roomId, AddTeamChatRoomMembersDto dto, CancellationToken ct = default);
    Task<ApiResponse<IReadOnlyList<TeamChatRoomMemberDto>>> GetTeamChatRoomMembersAsync(Guid teamId, Guid roomId, CancellationToken ct = default);
    Task<ApiResponse<IReadOnlyList<TeamReviewDto>>> GetReviewsAsync(Guid teamId, CancellationToken ct = default);
    Task<ApiResponse<TeamReviewDto>> AddReviewAsync(Guid teamId, CreateTeamFeedbackRequestDto request, CancellationToken ct = default);
    Task<Result<PaginatedResult<TeamProjectEarningsDto>>> GetTeamProjectEarnings(TeamProjectsFilter teamProjectsFilter);

    // For team projects
    Task<ApiResponse<IEnumerable<TeamProjectCardDto>>> GetTeamProjectsAsync(Guid teamId, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<ProjectMemberDto>>> GetProjectMembersAsync(Guid projectId, CancellationToken ct = default);
    Task<ApiResponse<ProjectMemberDto>> AssignProjectMemberAsync(Guid projectId, AssignProjectMemberDto dto, CancellationToken ct = default);
    Task<ApiResponse> RemoveProjectMemberAsync(Guid projectId, Guid userId, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<MilestoneAssignmentDto>>> GetMilestoneAssignmentsAsync(Guid milestoneId, CancellationToken ct = default);
    Task<ApiResponse> SetMilestoneAssignmentsAsync(Guid milestoneId, SetMilestoneAssignmentsDto dto, CancellationToken ct = default);
    Task<ApiResponse<IEnumerable<MilestoneAssigneeDto>>> GetMilestoneAssigneesAsync(Guid milestoneId, CancellationToken ct = default);
}
