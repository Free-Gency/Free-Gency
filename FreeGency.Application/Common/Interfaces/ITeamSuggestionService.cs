
namespace FreeGency.Application.Common.Interfaces;

public interface ITeamSuggestionService
{
    Task<ApiResponse<TeamSuggestionResponseDto>> SuggestTeamsForDeveloperAsync(int topK = 10, CancellationToken ct = default);
}
