
using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;

namespace FreeGency.Domain.Interfaces.Repositories.Reviews;

public interface IReviewRepository : IGenericRepository<Review>
{
    Task<IReadOnlyList<Review>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Review>> GetByProjectIdWithDetailsAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Review>> GetByRevieweeAsync(RevieweeType type, Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Review>> GetByReviewerAsync(Guid reviewerUserId, CancellationToken cancellationToken = default);


    Task<bool> HasReviewAsync(Guid projectId, Guid reviewerUserId, CancellationToken cancellationToken = default);

    Task<double> GetAverageForRevieweeAsync(RevieweeType type, Guid id, CancellationToken cancellationToken = default);


}
