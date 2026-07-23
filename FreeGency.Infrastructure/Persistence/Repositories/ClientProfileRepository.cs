using FreeGency.Domain.Entities;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories;

public sealed class ClientProfileRepository : GenericRepository<ClientProfile>, IClientProfileRepository
{
    public ClientProfileRepository(ApplicationDbContext context) : base(context) { }

    public async Task<bool> ExistsForUserAsync(Guid userId, CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .AnyAsync(cp => cp.UserId == userId, ct);

    public async Task<ClientProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(cp => cp.UserId == userId, ct);

    public async Task UpdateRatingAsync(Guid userId, decimal avg, int count, CancellationToken ct = default)
        => await _dbSet
            .Where(cp => cp.UserId == userId)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(cp => cp.AverageRating, avg)
                 .SetProperty(cp => cp.RatingCount, count), ct);

    public async Task AddInterestsAsync(Guid userId, IEnumerable<Guid> categoryIds, CancellationToken ct = default)
    {
        var profile = await GetByUserIdAsync(userId, ct)
            ?? throw new InvalidOperationException("Client profile not found.");

        var ids = categoryIds?.Distinct().ToList() ?? [];
        if (ids.Count == 0)
            return;

        var existingCategoryIds = await _context.UserInterests
            .Where(ui => ui.ClientProfileId == profile.Id)
            .Select(ui => ui.CategoryId)
            .ToListAsync(ct);

        var toAdd = ids
            .Except(existingCategoryIds)
            .Select(categoryId => new UserInterest
            {
                Id = Guid.NewGuid(),
                ClientProfileId = profile.Id,
                CategoryId = categoryId
            })
            .ToList();

        if (toAdd.Count == 0)
            return;

        await _context.UserInterests.AddRangeAsync(toAdd, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task ReplaceInterestsAsync(Guid userId, IEnumerable<Guid> categoryIds, CancellationToken ct = default)
    {
        var profile = await GetByUserIdAsync(userId, ct)
            ?? throw new InvalidOperationException("Client profile not found.");

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        try
        {
            await _context.UserInterests
                .Where(ui => ui.ClientProfileId == profile.Id)
                .ExecuteDeleteAsync(ct);

            if (categoryIds != null && categoryIds.Any())
            {
                var newInterests = categoryIds.Distinct().Select(categoryId => new UserInterest
                {
                    Id = Guid.NewGuid(),
                    ClientProfileId = profile.Id,
                    CategoryId = categoryId
                });

                await _context.UserInterests.AddRangeAsync(newInterests, ct);
            }

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task AddSpecialtiesAsync(Guid userId, IEnumerable<Guid> specialtyIds, CancellationToken ct = default)
    {
        var profile = await GetByUserIdAsync(userId, ct)
            ?? throw new InvalidOperationException("Client profile not found.");

        var ids = specialtyIds?.Distinct().ToList() ?? [];
        if (ids.Count == 0)
            return;

        var existingSpecialtyIds = await _context.UserSpecialties
            .Where(us => us.ClientProfileId == profile.Id)
            .Select(us => us.SpecialtyId)
            .ToListAsync(ct);

        var toAdd = ids
            .Except(existingSpecialtyIds)
            .Select(specialtyId => new UserSpecialty
            {
                Id = Guid.NewGuid(),
                ClientProfileId = profile.Id,
                SpecialtyId = specialtyId
            })
            .ToList();

        if (toAdd.Count == 0)
            return;

        await _context.UserSpecialties.AddRangeAsync(toAdd, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task ReplaceSpecialtiesAsync(Guid userId, IEnumerable<Guid> specialtyIds, CancellationToken ct = default)
    {
        var profile = await GetByUserIdAsync(userId, ct)
            ?? throw new InvalidOperationException("Client profile not found.");

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        try
        {
            await _context.UserSpecialties
                .Where(us => us.ClientProfileId == profile.Id)
                .ExecuteDeleteAsync(ct);

            if (specialtyIds != null && specialtyIds.Any())
            {
                var newSpecialties = specialtyIds.Distinct().Select(specialtyId => new UserSpecialty
                {
                    Id = Guid.NewGuid(),
                    ClientProfileId = profile.Id,
                    SpecialtyId = specialtyId
                });

                await _context.UserSpecialties.AddRangeAsync(newSpecialties, ct);
            }

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task ReplaceSkillsAsync(Guid userId, IEnumerable<Guid> skillIds, CancellationToken ct = default)
    {
        var profile = await GetByUserIdAsync(userId, ct)
            ?? throw new InvalidOperationException("Client profile not found.");

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        try
        {
            await _context.UserSkills
                .Where(us => us.ClientProfileId == profile.Id)
                .ExecuteDeleteAsync(ct);

            if (skillIds != null && skillIds.Any())
            {
                var newSkills = skillIds.Distinct().Select(skillId => new UserSkill
                {
                    Id = Guid.NewGuid(),
                    ClientProfileId = profile.Id,
                    SkillId = skillId
                });

                await _context.UserSkills.AddRangeAsync(newSkills, ct);
            }

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}
