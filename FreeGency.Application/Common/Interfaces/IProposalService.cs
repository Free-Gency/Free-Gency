

using FreeGency.Application.Features.Proposals.Dtos;

namespace FreeGency.Application.Common.Interfaces;

public interface IProposalService
{    
    Task<ApiResponse<PaginatedResult<ProposalDto>>> BrowseAsync(FilterProposalDto filter, CancellationToken ct = default);
    Task<ApiResponse<ProposalDto>> GetByIdAsync(Guid id, CancellationToken ct = default);


    Task<ApiResponse> CreateAsync(CreateProposalDto dto, CancellationToken ct = default);
    Task<ApiResponse> UpdateAsync(UpdateProposalDto dto, CancellationToken ct = default);


    Task<ApiResponse<bool>> WithdrawAsync(Guid proposalId, CancellationToken ct = default);
    Task<ApiResponse> AcceptAsync(Guid proposalId, CancellationToken ct = default);
    Task<ApiResponse> RejectAsync(Guid proposalId, CancellationToken ct = default);


    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default);
}