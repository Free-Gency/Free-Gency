namespace FreeGency.Domain.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        // Reads
        Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
        Task<T?> GetEntityWithSpec(ISpecifiaction<T> spec);
        Task<List<T>> ListAsync(ISpecifiaction<T> spec);
        Task<int> CountAsync(ISpecifiaction<T> Spec);
        IQueryable<T> Query();
        // Writes
        Task AddAsync(T entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        void Delete(T entity);
    }
}
