namespace Despro.Framework.Base.IBaseServices;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken token = new CancellationToken());
    void BeginTransaction();
    Task BeginTransactionAsync(CancellationToken token = new CancellationToken());
    void CommitTransaction();
    Task CommitTransactionAsync(CancellationToken token = new CancellationToken());
    void RollbackTransaction();
    Task RollbackTransactionAsync(CancellationToken token = new CancellationToken());
    Task ExecuteTransactionAsync(Action action, CancellationToken token = new CancellationToken());
    Task ExecuteTransactionAsync(Func<Task> action, CancellationToken token = new CancellationToken());
    Task<T> ExecuteTransactionAsync<T>(Func<Task<T>> action, CancellationToken token = new CancellationToken());
    void Detach<TEntity>(TEntity entity) where TEntity : class;
}