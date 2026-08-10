
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories.Reviews;

public class ReviewRepository : GenericRepository<Review>, IReviewRepository
{
    public ReviewRepository(ApplicationDbContext context) : base(context) { }



    public async Task<IReadOnlyList<Review>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.ProjectId == projectId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Review>> GetByProjectIdWithDetailsAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.ProjectId == projectId)
            .Include(r => r.ReviewerUser)
            .Include(r => r.Project)
            .ToListAsync(cancellationToken);
    }


    public async Task<IReadOnlyList<Review>> GetByRevieweeAsync(RevieweeType type, Guid id, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(r => r.ReviewerUser)
                .ThenInclude(u => u.ClientProfile)
            .Include(r => r.ReviewerUser)
                .ThenInclude(u => u.DeveloperProfile)
            .AsQueryable();

        if (type == RevieweeType.Team)
            query = query.Where(r => r.RevieweeType == type && r.RevieweeTeamId == id);
        else if (type == RevieweeType.User)
            query = query.Where(r => r.RevieweeType == type && r.RevieweeUserId == id);

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Take(8)
            .ToListAsync(cancellationToken);
    }


    public async Task<IReadOnlyList<Review>> GetByReviewerAsync(Guid reviewerUserId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.ReviewerUserId == reviewerUserId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasReviewAsync(Guid projectId, Guid reviewerUserId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(r => r.ProjectId == projectId && r.ReviewerUserId == reviewerUserId, cancellationToken);
    }

    public async Task<double> GetAverageForRevieweeAsync(RevieweeType type, Guid id, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        if (type == RevieweeType.Team)
            query = query.Where(r => r.RevieweeType == type && r.RevieweeTeamId == id);
        else if (type == RevieweeType.User)
            query = query.Where(r => r.RevieweeType == type && r.RevieweeUserId == id);

        return await query.AverageAsync(r => (double?)r.Rating ?? 0, cancellationToken);
    }

}
