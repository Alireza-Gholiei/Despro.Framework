using Despro.Framework.Base.BaseModels;
using System.Linq.Expressions;

namespace Despro.Framework.Base.IBaseServices;

public interface IBasePublisherRepository<TEntity> : IDisposable, IAsyncDisposable where TEntity : Aggregate
{
    Task AddAsync(TEntity? entity, CancellationToken cancellationToken = new CancellationToken());
    Task UpdateAsync(TEntity? entity);
    /// <summary>
    /// Partially updates an entity.
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="updateAction">Action to update entity instance</param>
    /// <param name="updatedProperties">Updated properties</param>
    /// <returns>Task</returns>
    /// <example>
    /// <code>
    /// await _productRepository.UpdatePartialAsync(
    ///     id: 5,
    ///     entity =>
    ///     {
    ///         entity.SetCreate(DateTime.Now.Ticks, _authService.GetUserId()); 
    ///         entity.GetType().GetProperty("Price")?.SetValue(entity, 15000);
    ///         entity.GetType().GetProperty("Stock")?.SetValue(entity, 10);
    ///     },
    ///     p => p.Price,
    ///     p => p.Stock
    /// );
    /// </code>
    /// </example>
    Task UpdatePartialAsync(long id, Action<TEntity> updateAction, params Expression<Func<TEntity, object>>[] updatedProperties);
    Task RemoveAsync(long id, CancellationToken cancellationToken = new CancellationToken());
    Task RemoveAsync(TEntity? entity, CancellationToken cancellationToken = new CancellationToken());
    Task RemoveTrackingAsync(TEntity? entity, CancellationToken cancellationToken = new CancellationToken());
    Task HardDeleteAsync(long id, CancellationToken cancellationToken = new CancellationToken());
    Task HardDeleteAsync(TEntity? entity, CancellationToken cancellationToken = new CancellationToken());
}