using Despro.Framework.Base.BaseModels;
using System.Data;
using System.Linq.Expressions;

namespace Despro.Framework.Base.IBaseServices.IDbServices;

public interface IDapperRepository<TEntity> where TEntity : Aggregate
{
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<IEnumerable<TEntity>> GetPagedAsync(int page, int pageSize, string orderBy = nameof(Aggregate.Id));
    Task<TEntity?> GetByIdAsync(long id);
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
    Task<int> ExecuteRawQueryAsync(string sql, object? parameters = null);
    Task<IEnumerable<T>> ExecuteJoinQueryAsync<T>(string joinSql, object? parameters = null);
    Task<IEnumerable<T>> ExecuteSPAsync<T>(string procedureName, object? parameters = null);
    Task<T?> ExecuteSPSingleAsync<T>(string procedureName, object? parameters = null);
    Task<int> ExecuteSPNonQueryAsync(string procedureName, object? parameters = null);
    Task<(T Result, Dictionary<string, object> OutputParams)> ExecuteSPWithOutputsAsync<T>(
        string procedureName, object inputParams, Dictionary<string, DbType> outputParams);
}