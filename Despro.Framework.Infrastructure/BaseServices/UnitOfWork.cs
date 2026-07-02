using Despro.Framework.Base.BaseExceptions;
using Despro.Framework.Base.IBaseServices;
using Despro.Framework.Infrastructure.BaseServices.IDIContainer;
using Despro.Framework.Infrastructure.Contexts;
using Despro.Framework.Infrastructure.InfrastructureExceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Despro.Framework.Infrastructure.BaseServices;

public class UnitOfWork(EfBaseContext dbContext, IRepositoryServices repositoryServices) : IUnitOfWork
{
    private readonly EfBaseContext _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    private IDbContextTransaction? _transaction;
    private bool _disposed;

    public int SaveChanges()
    {
        var result = _context.SaveChanges();

        repositoryServices.LoggingContext.FlushLogs();

        return result;
    }

    public async Task<int> SaveChangesAsync(CancellationToken token = default)
    {
        var result = await _context.SaveChangesAsync(token);

        await repositoryServices.LoggingContext.FlushLogsAsync();

        return result;
    }

    public void BeginTransaction()
    {
        _transaction = _context.Database.CurrentTransaction ?? _context.Database.BeginTransaction();
    }

    public async Task BeginTransactionAsync(CancellationToken token = default)
    {
        _transaction = _context.Database.CurrentTransaction ?? await _context.Database.BeginTransactionAsync(token);
    }

    public void CommitTransaction()
    {
        if (_transaction == null) return;
        try
        {
            _context.SaveChanges();
            _transaction.Commit();
            repositoryServices.LoggingContext.FlushLogs();
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public async Task CommitTransactionAsync(CancellationToken token = default)
    {
        if (_transaction == null) return;
        try
        {
            await _context.SaveChangesAsync(token);
            await _transaction.CommitAsync(token);
            await repositoryServices.LoggingContext.FlushLogsAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void RollbackTransaction()
    {
        var tx = _transaction ?? _context.Database.CurrentTransaction;
        if (tx == null) return;

        tx.Rollback();
        tx.Dispose();
        _transaction = null;

        _context.ChangeTracker.Clear();
    }

    public async Task RollbackTransactionAsync(CancellationToken token = default)
    {
        var tx = _transaction ?? _context.Database.CurrentTransaction;
        if (tx == null) return;

        await tx.RollbackAsync(token);
        await tx.DisposeAsync();
        _transaction = null;

        _context.ChangeTracker.Clear();
    }

    public async Task ExecuteTransactionAsync(Action action, CancellationToken token = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(token);

        try
        {
            action();
            await _context.SaveChangesAsync(token);
            await transaction.CommitAsync(token);
            await repositoryServices.LoggingContext.FlushLogsAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(token);
            throw new Exception("Can't Execute Transaction", ex);
        }
    }

    public async Task ExecuteTransactionAsync(Func<Task> action, CancellationToken token = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(token);
        try
        {
            await action();
            await _context.SaveChangesAsync(token);
            await transaction.CommitAsync(token);
            await repositoryServices.LoggingContext.FlushLogsAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(token);
            throw new Exception("Can't Execute Transaction", ex);
        }
    }

    public async Task<T> ExecuteTransactionAsync<T>(Func<Task<T>> action, CancellationToken token = new CancellationToken())
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(token);
        try
        {
            var result = await action();

            await _context.SaveChangesAsync(token);
            await transaction.CommitAsync(token);
            await repositoryServices.LoggingContext.FlushLogsAsync();

            return result;
        }
        catch (BaseException ex)
        {
            await transaction.RollbackAsync(token);
            throw new BaseRepositoryException(ex.Message);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(token);
            throw new Exception("Can't Execute Transaction", ex);
        }
    }

    public void Detach<TEntity>(TEntity entity) where TEntity : class
    {
        _context.Entry(entity).State = EntityState.Detached;
    }

    public void Dispose()
    {
        if (_disposed) return;

        _transaction?.Dispose();

        _context.Dispose();
        _disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        if (_transaction != null)
            await _transaction.DisposeAsync();

        await _context.DisposeAsync();
        _disposed = true;
    }
}
