using System;
using System.Collections.Generic;
using System.Text;
using FreeGency.Domain.Entities;
using FreeGency.Domain.Enums;
using FreeGency.Domain.Interfaces.Repositories;
using FreeGency.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FreeGency.Infrastructure.Persistence.Repositories
{
    public class ProjectRepository : GenericRepository<Project>,IProjectRepository
    {
        public ProjectRepository(ApplicationDbContext context):base(context) 
        {            
        }
        public async Task<IEnumerable<Project>> GetByClientIdAsync(Guid clientId, ProjectStatus? status = null,CancellationToken ct = default)
        {
            IQueryable<Project> query = _dbSet.AsNoTracking().AsSplitQuery().Include(p => p.Category)
                .Include(p => p.Specialty).Include(p => p.ProjectSkills).ThenInclude(ps => ps.Skill).Where(p => p.ClientId == clientId);
            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }
            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync(ct);
        }
        public async Task<IEnumerable<Project>> SearchOpenAsync(string? keyword,Guid? categoryId,Guid? specialtyId,decimal? minBudget,decimal? maxBudget,CancellationToken ct = default)
        {
            IQueryable<Project> query = _dbSet.AsNoTracking().AsSplitQuery().Include(p => p.Client).Include(p => p.Category).Include(p => p.Specialty) .Include(p => p.ProjectSkills)
                    .ThenInclude(ps => ps.Skill).Where(p => p.Status == ProjectStatus.Open);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p => EF.Functions.Like(p.Title, $"%{keyword}%") ||EF.Functions.Like(p.Description, $"%{keyword}%"));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (specialtyId.HasValue)
            {
                query = query.Where(p => p.SpecialtyId == specialtyId.Value);
            }

            if (minBudget.HasValue)
            {
                query = query.Where(p => p.BudgetMax >= minBudget.Value);
            }

            if (maxBudget.HasValue)
            {
                query = query.Where(p => p.BudgetMin <= maxBudget.Value);
            }

            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync(ct);
        }


        public async Task AddWithSkillsAsync(Project project,IEnumerable<Guid> skillIds,CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(project);

            if (project.Id == Guid.Empty)
                project.Id = Guid.NewGuid();

            await _dbSet.AddAsync(project, ct);

            var skills = skillIds.Distinct().ToList();

            if (!skills.Any())
                return;

            var projectSkills = skills.Select(skillId => new ProjectSkill
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                SkillId = skillId
            });

            await _context.Set<ProjectSkill>().AddRangeAsync(projectSkills, ct);
        }
        public async Task UpdateStatusAsync(Guid id,ProjectStatus status,CancellationToken ct = default)
        {
            var project = await _dbSet.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (project is null)
                throw new KeyNotFoundException("Project not found.");
            project.Status = status;
            if (status == ProjectStatus.Completed)
            {
                project.CompletedAt = DateTime.UtcNow;
            }
            _dbSet.Update(project);
        }
        public async Task SetAssigneeAsync(Guid id,Guid? userId,Guid? teamId,CancellationToken ct = default)
        {
            var project = await _dbSet.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (project is null)
                throw new KeyNotFoundException("Project not found.");
            project.AssignedUserId = userId;
            project.AssignedTeamId = teamId;
            project.Status = ProjectStatus.InProgress;
            _dbSet.Update(project);
        }
        public async Task ReplaceSkillsAsync(Guid projectId,IEnumerable<Guid> skillIds,CancellationToken ct = default)
        {
            if (!await ExistsAsync(projectId, ct))
                throw new KeyNotFoundException("Project not found.");
            var oldSkills = await _context
                .Set<ProjectSkill>().Where(ps => ps.ProjectId == projectId).ToListAsync(ct);
            if (oldSkills.Count > 0)
            {
                _context.Set<ProjectSkill>().RemoveRange(oldSkills);
            }

            var skills = skillIds.Distinct().ToList();
            if (skills.Count == 0)
                return;
            var newSkills = skills.Select(skillId => new ProjectSkill
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                SkillId = skillId
            });
            await _context.Set<ProjectSkill>().AddRangeAsync(newSkills, ct);
        }
        public async Task SaveProjectAsync(Guid projectId,Guid userId,CancellationToken ct = default)
        {
            if (!await ExistsAsync(projectId, ct))
                throw new KeyNotFoundException("Project not found.");
            var exists = await _context.Set<SavedProject>().AsNoTracking().AnyAsync(x =>x.ProjectId == projectId &&x.UserId == userId,ct);
            if (exists)
                return;
            await _context.Set<SavedProject>().AddAsync(new SavedProject
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    UserId = userId
                }, ct);
        }
        public async Task UnsaveProjectAsync(Guid projectId,Guid userId,CancellationToken ct = default)
        {
            var savedProject = await _context.Set<SavedProject>().FirstOrDefaultAsync(x=>x.ProjectId == projectId&&x.UserId == userId,ct);
            if (savedProject is null)
                return;
            _context.Set<SavedProject>().Remove(savedProject);
        }
        public async Task<IEnumerable<Project>> GetSavedByUserAsync(Guid userId,CancellationToken ct = default)
        {
            return await _context.Set<SavedProject>().AsNoTracking().AsSplitQuery()
                .Where(x => x.UserId == userId)
                .Include(x => x.Project)
                    .ThenInclude(x => x.Client)
                .Include(x => x.Project)
                    .ThenInclude(x => x.Category)
                .Include(x => x.Project)
                    .ThenInclude(x => x.Specialty)
                .Include(x => x.Project)
                    .ThenInclude(x => x.ProjectSkills)
                        .ThenInclude(x => x.Skill)
                .Select(x => x.Project)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);
        }

    }
}
