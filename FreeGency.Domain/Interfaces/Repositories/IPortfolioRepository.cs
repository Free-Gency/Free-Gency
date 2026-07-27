using FreeGency.Domain.Entities;

namespace FreeGency.Domain.Interfaces.Repositories
{
    public interface IPortfolioRepository : IGenericRepository<PortfolioProject>
    {
        #region Queries

        Task<IEnumerable<PortfolioProject>> GetDeveloperPortfolioAsync(
            Guid developerId,
            CancellationToken ct = default);

        Task<IEnumerable<PortfolioProject>> GetTeamPortfolioAsync(
            Guid teamId,
            CancellationToken ct = default);

        IQueryable<PortfolioProject> GetInspirationQuery(
            Guid? categoryId,
            string? search);

        Task RecordViewAsync(
            Guid userId,
            Guid portfolioProjectId,
            CancellationToken ct = default);

        Task<IReadOnlyList<(PortfolioProject Project, DateTime ViewedAt)>> GetRecentlyViewedByUserAsync(
            Guid userId,
            int take,
            CancellationToken ct = default);

        Task<IReadOnlyList<PortfolioFeedback>> GetFeedbackAsync(
            Guid portfolioProjectId,
            int take,
            CancellationToken ct = default);

        Task<bool> HasFeedbackAsync(
            Guid portfolioProjectId,
            Guid reviewerUserId,
            CancellationToken ct = default);

        Task AddFeedbackAsync(
            PortfolioFeedback feedback,
            CancellationToken ct = default);

        Task<PortfolioProject?> GetDetailsAsync(
            Guid id,
            CancellationToken ct = default);

        Task<PortfolioProject?> GetPublicDetailsAsync(
            Guid id,
            CancellationToken ct = default);

        Task<bool> IsOwnerAsync(
            Guid portfolioProjectId,
            Guid userId,
            CancellationToken ct = default);

        Task<bool> IsTeamOwnerAsync(
            Guid portfolioProjectId,
            Guid teamId,
            CancellationToken ct = default);

        #endregion


        #region Commands

        Task AddWithSkillsAsync(
            PortfolioProject project,
            IEnumerable<Guid> skillIds,
            CancellationToken ct = default);

        Task ReplaceSkillsAsync(
            Guid portfolioProjectId,
            IEnumerable<Guid> skillIds,
            CancellationToken ct = default);

        Task AddImagesAsync(
            Guid portfolioProjectId,
            IEnumerable<string> imageUrls,
            CancellationToken ct = default);

        Task DeleteImageAsync(
            Guid imageId,
            CancellationToken ct = default);

        Task<PortfolioImage?> GetImageByIdAsync(
            Guid imageId,
            CancellationToken ct = default);

        Task DeleteImagesAndSkillsAsync(
            Guid portfolioProjectId,
            CancellationToken ct = default);

        #endregion

        new IQueryable<PortfolioProject> Query();
    }
}
