using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Interfaces.Repositories.Portfolio;

public interface IPortfolioProjectRepository : IGenericRepository<PortfolioProject>
{
    Task<IReadOnlyList<PortfolioProject>> GetByOwnerIdAsync(owner ownerType, Guid ownerId, CancellationToken ct = default);

    Task<IReadOnlyList<PortfolioProject>> GetByOwnerIdWithDetailsAsync(owner ownerType, Guid ownerId, CancellationToken ct = default);

    Task<PortfolioProject?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);

    Task AddWithDetailsAsync(PortfolioProject project, IEnumerable<(string ImageUrl, int SortOrder)> images,IEnumerable<Guid> skillIds,CancellationToken ct = default);
    
    Task ReplaceImagesAsync(Guid portfolioProjectId, IEnumerable<(string ImageUrl, int SortOrder)> images, CancellationToken ct = default);
    
    Task ReplaceSkillsAsync(Guid portfolioProjectId, IEnumerable<Guid> skillIds, CancellationToken ct = default);
}
