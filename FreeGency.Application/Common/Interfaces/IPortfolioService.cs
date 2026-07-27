namespace FreeGency.Application.Common.Interfaces
{
    public interface IPortfolioService
    {
        #region Developer Portfolio

        Task<ApiResponse<Guid>> CreateAsync(
            CreatePortfolioProjectRequestDto request,
            CancellationToken ct = default);

        Task<ApiResponse> UpdateAsync(
            Guid id,
            UpdatePortfolioProjectRequestDto request,
            CancellationToken ct = default);

        Task<ApiResponse> DeleteAsync(
            Guid id,
            CancellationToken ct = default);

        Task<ApiResponse<IEnumerable<PortfolioProjectDto>>> GetMineAsync(
            CancellationToken ct = default);

        Task<ApiResponse<PortfolioProjectDetailsDto>> GetDetailsAsync(
            Guid id,
            CancellationToken ct = default);

        Task<ApiResponse<PortfolioProjectDetailsDto>> GetPublicDetailsAsync(
            Guid id,
            CancellationToken ct = default);

        Task<ApiResponse<IEnumerable<PortfolioProjectDto>>> GetDeveloperPortfolioAsync(
            Guid developerId,
            CancellationToken ct = default);

        Task<ApiResponse<IEnumerable<PortfolioProjectDto>>> GetInspirationAsync(
            Guid? categoryId = null,
            string? search = null,
            int take = 24,
            CancellationToken ct = default);

        Task<ApiResponse> RecordViewAsync(
            Guid portfolioProjectId,
            CancellationToken ct = default);

        Task<ApiResponse<IEnumerable<RecentlyViewedPortfolioDto>>> GetRecentlyViewedAsync(
            int take = 5,
            CancellationToken ct = default);

        Task<ApiResponse<OwnerReviewDto>> AddFeedbackAsync(
            Guid portfolioProjectId,
            CreatePortfolioFeedbackRequestDto request,
            CancellationToken ct = default);

        Task<ApiResponse> ReplaceSkillsAsync(
            Guid portfolioProjectId,
            IEnumerable<Guid> skillIds,
            CancellationToken ct = default);

        Task<ApiResponse> UploadImagesAsync(
            Guid portfolioProjectId,
            IEnumerable<IFormFile> images,
            CancellationToken ct = default);

        Task<ApiResponse> DeleteImageAsync(
            Guid portfolioId,
            Guid imageId,
            CancellationToken ct = default);

        #endregion


        #region Team Portfolio

        Task<ApiResponse<Guid>> CreateForTeamAsync(
            Guid teamId,
            CreatePortfolioProjectRequestDto request,
            CancellationToken ct = default);

        Task<ApiResponse> UpdateForTeamAsync(
            Guid teamId,
            Guid id,
            UpdatePortfolioProjectRequestDto request,
            CancellationToken ct = default);

        Task<ApiResponse> DeleteForTeamAsync(
            Guid teamId,
            Guid id,
            CancellationToken ct = default);

        Task<ApiResponse<IEnumerable<PortfolioProjectDto>>> GetTeamPortfolioAsync(
            Guid teamId,
            CancellationToken ct = default);

        Task<ApiResponse> ReplaceTeamSkillsAsync(
            Guid teamId,
            Guid portfolioProjectId,
            IEnumerable<Guid> skillIds,
            CancellationToken ct = default);

        Task<ApiResponse> UploadTeamImagesAsync(
            Guid teamId,
            Guid portfolioProjectId,
            IEnumerable<IFormFile> images,
            CancellationToken ct = default);

        Task<ApiResponse> DeleteTeamImageAsync(
            Guid teamId,
            Guid imageId,
            CancellationToken ct = default);

        #endregion
    }
}
