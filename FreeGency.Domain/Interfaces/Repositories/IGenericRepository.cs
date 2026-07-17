namespace FreeGency.Domain.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        // Reads
        Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);

        // Writes
        Task AddAsync(T entity, CancellationToken ct = default);
        void Update(T entity);
        void Delete(T entity);
    }
}
