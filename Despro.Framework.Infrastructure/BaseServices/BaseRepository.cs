using Despro.Framework.Base.BaseModels;
using Despro.Framework.Base.BaseModels.GridData;
using Despro.Framework.Base.IBaseServices;
using Despro.Framework.Infrastructure.BaseServices.IDIContainer;
using Despro.Framework.Infrastructure.Contexts;
using Despro.Framework.Infrastructure.InfrastructureExceptions;
using Despro.Framework.Infrastructure.InfrastructureIServices;
using Despro.Framework.Infrastructure.InfrastructureModels.DbModels;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Despro.Framework.Infrastructure.BaseServices;

public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity
{
    private readonly EfBaseContext _context;
    private readonly DbSet<TEntity> _dbTable;
    private readonly ILoggingContext _loggingContext;
    private readonly IAuthService _authService;

    protected BaseRepository(EfBaseContext context, IRepositoryServices repositoryServices)
    {
        _context = context;
        _authService = repositoryServices.AuthService;
        _loggingContext = repositoryServices.LoggingContext;

        _dbTable = _context.Set<TEntity>();
    }

    #region Publisher
    public async Task AddAsync(TEntity? entity, CancellationToken cancellationToken = default)
    {
        if (entity == null) return;

        ApplyAuditRecursivelyOptimized(entity);

        await _dbTable.AddAsync(entity, cancellationToken);
        _loggingContext.AddLog(typeof(TEntity).Name, OperationLogType.Add, entity);
    }

    public Task UpdateAsync(TEntity? entity)
    {
        try
        {
            if (entity == null) return Task.CompletedTask;

            if (_dbTable.Entry(entity).State == EntityState.Unchanged)
            {
                _dbTable.Entry(entity).State = EntityState.Detached;
            }

            ApplyAuditRecursivelyOptimized(entity);

            _dbTable.Update(entity);

            _loggingContext.AddLog(typeof(TEntity).Name, OperationLogType.Update, entity);

            return Task.CompletedTask;
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new BaseRepositoryException("رکورد توسط کاربر دیگری تغییر کرده است. لطفاً صفحه را رفرش کنید.");
        }
    }

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
    public async Task UpdatePartialAsync(long id, Action<TEntity> updateAction, params Expression<Func<TEntity, object>>[] updatedProperties)
    {
        try
        {
            var entity = await _dbTable.FindAsync([id]);
            if (entity == null)
                return;

            updateAction(entity);

            ApplyAuditRecursivelyOptimized(entity);
            //ApplyAuditRecursively(entity, e => e.SetUpdate(DateTime.Now.Ticks, _authService.GetUserId()));

            _context.ChangeTracker.DetectChanges();

            if (_context.Entry(entity).Metadata.FindProperty("RowVersion") != null)
            {
                var dbValue = await _dbTable.AsNoTracking()
                    .Where(e => e.Id == id)
                    .Select(e => EF.Property<byte[]>(e, "RowVersion"))
                    .FirstOrDefaultAsync();

                if (dbValue != null)
                    _context.Entry(entity).Property("RowVersion").OriginalValue = dbValue;
            }

            if (updatedProperties is { Length: > 0 })
            {
                foreach (var property in updatedProperties)
                {
                    _context.Entry(entity).Property(property).IsModified = true;
                }
            }
            else
            {
                _context.Entry(entity).State = EntityState.Modified;
            }
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new BaseRepositoryException("رکورد توسط کاربر دیگری تغییر کرده است. لطفاً صفحه را رفرش کنید.");
        }
    }


    public async Task RemoveAsync(long id, CancellationToken cancellationToken = default)
    {
        var deletedItem = await GetByIdAsync(id, cancellationToken);
        if (deletedItem == null)
            return;

        ApplyAuditRecursivelyOptimized(deletedItem, isDelete: true);

        _dbTable.Update(deletedItem);
        _loggingContext.AddLog(typeof(TEntity).Name, OperationLogType.Delete, deletedItem);
    }

    public Task RemoveAsync(TEntity? entity, CancellationToken cancellationToken = new CancellationToken())
    {
        if (entity is null)
            return Task.CompletedTask;

        ApplyAuditRecursivelyOptimized(entity, isDelete: true);

        _dbTable.Update(entity);
        _loggingContext.AddLog(typeof(TEntity).Name, OperationLogType.Delete, entity);
        return Task.CompletedTask;
    }

    public Task RemoveTrackingAsync(TEntity? entity, CancellationToken cancellationToken = new CancellationToken())
    {
        if (entity is null)
            return Task.CompletedTask;

        ApplyAuditRecursivelyOptimized(entity, isDelete: true);

        _loggingContext.AddLog(typeof(TEntity).Name, OperationLogType.Delete, entity);
        return Task.CompletedTask;
    }

    public async Task HardDeleteAsync(long id, CancellationToken cancellationToken = new CancellationToken())
    {
        var deletedItem = await GetByIdAsync(id, cancellationToken);

        _dbTable.Remove(deletedItem!);

        _loggingContext.AddLog(typeof(TEntity).Name, OperationLogType.Delete, deletedItem);
    }

    public Task HardDeleteAsync(TEntity? entity, CancellationToken cancellationToken = new CancellationToken())
    {
        if (entity is null)
            return Task.CompletedTask;

        _dbTable.Remove(entity);

        _loggingContext.AddLog(typeof(TEntity).Name, OperationLogType.Delete, entity);
        return Task.CompletedTask;
    }

    #endregion

    #region Read
    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? filter = null)
    {
        return filter == null
            ? await _dbTable.AnyAsync()
            : await _dbTable.AnyAsync(filter);
    }

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? filter = null)
    {
        return filter == null
            ? await _dbTable.CountAsync()
            : await _dbTable.CountAsync(filter);
    }

    public async Task<TEntity> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await Table().FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);
    }

    public async Task<TEntity> GetTrackingAsync(long id, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _dbTable.AsTracking();

        if (includes is not { Length: > 0 })
            return await query.FirstOrDefaultAsync(t => t.Id.Equals(id), cancellationToken);

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(t => t.Id.Equals(id), cancellationToken);
    }

    public async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var list = await Table().OrderByDescending(x => x.Id).ToListAsync(cancellationToken);

        return list;
    }

    public int GetFilterCount(BaseGrid baseGrid, Expression<Func<TEntity, bool>>? filter = null)
    {
        var query = _dbTable.Where(x => !x.IsDelete).AsNoTracking();

        if (baseGrid.FilterParam != null && baseGrid.FilterParam.Any())
        {
            query = query.FilterList(baseGrid);
        }

        if (filter != null)
        {
            query = query.Where(filter);
        }

        return query.Count();
    }

    public IQueryable<TEntity> GetFilterPaging(BaseGrid baseGrid)
    {
        return baseGrid.FilterParam != null && baseGrid.FilterParam.Any()
            ? _dbTable.FilterPagingList(baseGrid)
                .AsNoTracking()
            : _dbTable.PagingList(baseGrid)
                .AsNoTracking();
    }

    public async Task<GridData<TDto>> GetFilterPagingDtoAsync<TDto>(BaseGrid baseGrid, CancellationToken cancellationToken = default)
    {
        var query = GetFilterPaging(baseGrid);

        var queryList = await query.ToListAsync(cancellationToken);

        var mapedList = queryList.Adapt<List<TDto>>();

        var dto = new GridData<TDto>(mapedList, baseGrid, GetFilterCount(baseGrid));

        return dto;
    }

    public IQueryable<TEntity> Table()
    {
        return _dbTable.AsNoTracking();
    }

    public IQueryable<TEntity> TableWithDelete()
    {
        return _dbTable.IgnoreQueryFilters().AsNoTracking();
    }

    public IQueryable<TNewEntity> Context<TNewEntity>() where TNewEntity : BaseEntity
    {
        return _context.Set<TNewEntity>().AsNoTracking();
    }

    public IQueryable<TNewEntity> ContextWithDelete<TNewEntity>() where TNewEntity : BaseEntity
    {
        return _context.Set<TNewEntity>().IgnoreQueryFilters().AsNoTracking();
    }
    #endregion

    #region Tools
    //private void ApplyAuditToNavigations(object entity, Action<object> auditAction)
    //{
    //    if (entity == null)
    //        return;

    //    var entityType = entity.GetType();
    //    var props = entityType.GetProperties();

    //    foreach (var prop in props)
    //    {
    //        if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string))
    //        {
    //            if (prop.GetValue(entity) is not IEnumerable collection) continue;
    //            foreach (var item in collection)
    //            {
    //                if (item == null) continue;
    //                auditAction(item);
    //                ApplyAuditToNavigations(item, auditAction);
    //            }
    //        }
    //        else if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
    //        {
    //            var nav = prop.GetValue(entity);
    //            if (nav == null) continue;
    //            auditAction(nav);
    //            ApplyAuditToNavigations(nav, auditAction);
    //        }
    //    }
    //}

    //private void ApplyAuditRecursivelyWithCreateUpdate(BaseEntity entity)
    //{
    //    ApplyAuditRecursively(entity, e =>
    //    {
    //        var entry = _context.Entry(e);

    //        if (entry.State == EntityState.Added || (entry.State == EntityState.Detached && e.Id == 0))
    //            e.SetCreate(DateTime.UtcNow.Ticks, _authService.GetUserId());
    //        else
    //            e.SetUpdate(DateTime.UtcNow.Ticks, _authService.GetUserId());
    //    });
    //}


    //private void ApplyAuditRecursively(object entity, Action<BaseEntity> auditAction, HashSet<object>? visited = null)
    //{
    //    if (entity is not BaseEntity baseEntity)
    //        return;

    //    visited ??= new HashSet<object>(ReferenceEqualityComparer.Instance);

    //    if (!visited.Add(entity))
    //        return;

    //    auditAction(baseEntity);

    //    var navProps = entity.GetType()
    //        .GetProperties()
    //        .Where(p =>
    //            (typeof(IEnumerable<BaseEntity>).IsAssignableFrom(p.PropertyType) ||
    //             typeof(BaseEntity).IsAssignableFrom(p.PropertyType)) &&
    //            p.GetValue(entity) != null);

    //    foreach (var prop in navProps)
    //    {
    //        var value = prop.GetValue(entity);

    //        switch (value)
    //        {
    //            case null:
    //                continue;

    //            case IEnumerable<BaseEntity> collection:
    //                foreach (var item in collection)
    //                    ApplyAuditRecursively(item, auditAction, visited);
    //                break;

    //            case BaseEntity singleEntity:
    //                ApplyAuditRecursively(singleEntity, auditAction, visited);
    //                break;
    //        }
    //    }
    //}

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _navigationPropertiesCache = new();

    private void ApplyAuditRecursivelyOptimized(BaseEntity entity, bool isDelete = false, HashSet<object>? visited = null)
    {
        if (entity == null) return;

        visited ??= new HashSet<object>(ReferenceEqualityComparer.Instance);

        if (!visited.Add(entity)) return;

        var entry = _context.Entry(entity);
        if (isDelete)
        {
            entity.SetDelete(DateTime.UtcNow.Ticks, _authService.GetUserId());
        }
        else if (entry.State == EntityState.Added || entry.State == EntityState.Detached && entity.Id == 0)
        {
            entity.SetCreate(DateTime.UtcNow.Ticks, _authService.GetUserId());
        }
        else
        {
            entity.SetUpdate(DateTime.UtcNow.Ticks, _authService.GetUserId());
        }

        var entityType = entity.GetType();
        var navProps = _navigationPropertiesCache.GetOrAdd(entityType, type =>
            type.GetProperties()
                .Where(p =>
                    (typeof(IEnumerable<BaseEntity>).IsAssignableFrom(p.PropertyType) ||
                     typeof(BaseEntity).IsAssignableFrom(p.PropertyType)) &&
                    p.GetValue(entity) != null)
                .ToArray()
        );

        foreach (var prop in navProps)
        {
            var value = prop.GetValue(entity);
            switch (value)
            {
                case null:
                    continue;

                case IEnumerable<BaseEntity> collection:
                    foreach (var item in collection)
                        ApplyAuditRecursivelyOptimized(item, isDelete, visited);
                    break;

                case BaseEntity singleEntity:
                    ApplyAuditRecursivelyOptimized(singleEntity, isDelete, visited);
                    break;
            }
        }
    }

    class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Instance = new();

        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
        public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}