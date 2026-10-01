using Despro.Framework.Base.BaseModels;
using Despro.Framework.Base.BaseModels.GridData;
using System.Linq.Expressions;

namespace Despro.Framework.Base.IBaseServices;

public interface IBaseReadRepository<TEntity, in TId> where TEntity : Aggregate<TId> where TId : notnull
{
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? filter = null);
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? filter = null);
    Task<TEntity> GetByIdAsync(TId id, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes);
    Task<TEntity> GetTrackingAsync(TId id, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes);
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    IQueryable<TEntity> GetFilterPaging(BaseGrid baseGrid);
    Task<GridData<TDto>> GetFilterPagingDtoAsync<TDto>(BaseGrid baseGrid, CancellationToken cancellationToken = default);
    int GetFilterCount(BaseGrid baseGrid, Expression<Func<TEntity, bool>>? filter = null);
    IQueryable<TEntity> Table();
    IQueryable<TEntity> TableWithDelete();
    IQueryable<TNewEntity> Context<TNewEntity, TId>() where TNewEntity : Aggregate<TId> where TId : notnull;
    IQueryable<TNewEntity> ContextWithDelete<TNewEntity, TId>() where TNewEntity : Aggregate<TId> where TId : notnull;
}