using Microsoft.EntityFrameworkCore.Storage;
using System.Collections;

namespace FreeGency.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly IServiceProvider _serviceProvider;
    private IDbContextTransaction? _currentTransaction;
    private Hashtable? _repositories;
    private bool _disposed;

    public UnitOfWork(ApplicationDbContext context, IServiceProvider serviceProvider)
    {
        _context = context;
        _serviceProvider = serviceProvider;
    }


    public TRepository Repository<TRepository, TEntity>()
        where TRepository : class
        where TEntity : class
    {
        _repositories ??= new Hashtable();

        var key = typeof(TRepository).FullName ?? typeof(TRepository).Name;
        if (!_repositories.Contains(key))
        {
            var repository = _serviceProvider.GetRequiredService<TRepository>();
            _repositories.Add(key, repository);
        }
        return (TRepository)_repositories[key]!;
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction != null)
            return;

        _currentTransaction = await _context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        try
        {
            await _context.SaveChangesAsync(ct);
            if (_currentTransaction != null)
                await _currentTransaction.CommitAsync(ct);
        }
        catch
        {
            await RollbackTransactionAsync(ct);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(ct);
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            if (_currentTransaction != null)
                await _currentTransaction.DisposeAsync();

            await _context.DisposeAsync();
            _repositories?.Clear();
        }
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _currentTransaction?.Dispose();
                _context.Dispose();
                _repositories?.Clear();
            }
            _disposed = true;
        }
    }
}
