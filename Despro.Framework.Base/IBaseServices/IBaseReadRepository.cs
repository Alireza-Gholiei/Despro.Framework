using Despro.Framework.Base.BaseModels;
using Despro.Framework.Base.BaseModels.GridData;
using System.Linq.Expressions;

namespace Despro.Framework.Base.IBaseServices;

public interface IBaseReadRepository<TEntity> : IDisposable, IAsyncDisposable where TEntity : BaseEntity
{
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? filter = null);
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? filter = null);
    Task<TEntity> GetByIdAsync(long id, CancellationToken cancellationToken = new CancellationToken());

    Task<TEntity> GetTrackingAsync(long id, CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includes);
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = new CancellationToken());
    IQueryable<TEntity> GetFilterPaging(BaseGrid baseGrid);
    Task<GridData<TDto>> GetFilterPagingDtoAsync<TDto>(BaseGrid baseGrid, CancellationToken cancellationToken = new CancellationToken());
    int GetFilterCount(BaseGrid baseGrid, Expression<Func<TEntity, bool>>? filter = null);
    IQueryable<TEntity> Table();
    IQueryable<TEntity> TableWithDelete();
    IQueryable<TNewEntity> Context<TNewEntity>() where TNewEntity : BaseEntity;
    IQueryable<TNewEntity> ContextWithDelete<TNewEntity>() where TNewEntity : BaseEntity;
}