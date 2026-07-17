using FreeGency.Domain.Interfaces;

namespace FreeGency.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        public Task BeginTransactionAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task CommitTransactionAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        public Task RollbackTransactionAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
