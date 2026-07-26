using Dapper;
using Despro.Framework.Base.BaseModels;
using Despro.Framework.Base.IBaseServices.IDbServices;
using Despro.Framework.Infrastructure.Contexts;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Despro.Framework.Infrastructure.BaseServices.DbServices;

public class DapperRepository<TEntity>(IDbConnection dbConnection, EfBaseContext context)
    : IDapperRepository<TEntity>
    where TEntity : Aggregate
{
    //private readonly string _tableName = context.Model.FindEntityType(typeof(TEntity))?.GetTableName()
    //                                     ?? throw new InvalidOperationException($"Table name for {typeof(TEntity).Name} not found.");

    private readonly string _tableName = "";

    private static string EscapeTableName(string tableName)
    {
        return $"[{tableName.Replace("]", "]]")}]";
    }

    private static (string Sql, DynamicParameters Parameters) BuildWhereClause(Expression<Func<TEntity, bool>> predicate)
    {
        var parameters = new DynamicParameters();
        var sqlBuilder = new StringBuilder();
        var paramCounter = 0;

        ParseExpression(predicate.Body);
        return (sqlBuilder.Length == 0 ? "1=1" : sqlBuilder.ToString(), parameters);

        void ParseExpression(Expression expression)
        {
            switch (expression)
            {
                case BinaryExpression binary:
                    {
                        var left = binary.Left;
                        var right = binary.Right;
                        var op = binary.NodeType switch
                        {
                            ExpressionType.Equal => "=",
                            ExpressionType.GreaterThan => ">",
                            ExpressionType.LessThan => "<",
                            ExpressionType.AndAlso => "AND",
                            ExpressionType.OrElse => "OR",
                            _ => throw new NotSupportedException($"عملگر {binary.NodeType} پشتیبانی نمی‌شود.")
                        };

                        sqlBuilder.Append("(");
                        ParseExpression(left);
                        sqlBuilder.Append($" {op} ");
                        ParseExpression(right);
                        sqlBuilder.Append(")");
                        break;
                    }
                case MethodCallExpression { Method.Name: "Contains", Object: MemberExpression member } methodCall when methodCall.Arguments[0] is ConstantExpression constant:
                    {
                        var paramName = $"@p{paramCounter++}";
                        sqlBuilder.Append($"{member.Member.Name} LIKE {paramName}");
                        parameters.Add(paramName, $"%{constant.Value}%");
                        break;
                    }
                case MethodCallExpression { Method.Name: "StartsWith", Object: MemberExpression memberStart } methodCall when methodCall.Arguments[0] is ConstantExpression constantStart:
                    {
                        var paramName = $"@p{paramCounter++}";
                        sqlBuilder.Append($"{memberStart.Member.Name} LIKE {paramName}");
                        parameters.Add(paramName, $"{constantStart.Value}%");
                        break;
                    }
                case MethodCallExpression { Method.Name: "EndsWith", Object: MemberExpression memberEnd } methodCall when methodCall.Arguments[0] is ConstantExpression constantEnd:
                    {
                        var paramName = $"@p{paramCounter++}";
                        sqlBuilder.Append($"{memberEnd.Member.Name} LIKE {paramName}");
                        parameters.Add(paramName, $"%{constantEnd.Value}");
                        break;
                    }
                case MethodCallExpression methodCall:
                    throw new NotSupportedException($"متد {methodCall.Method.Name} پشتیبانی نمی‌شود.");
                case MemberExpression member when expression.Type == typeof(bool):
                    {
                        var paramName = $"@p{paramCounter++}";
                        sqlBuilder.Append($"{member.Member.Name} = {paramName}");
                        parameters.Add(paramName, true);
                        break;
                    }
                case UnaryExpression { NodeType: ExpressionType.Not } unary:
                    sqlBuilder.Append("NOT ");
                    ParseExpression(unary.Operand);
                    break;
                default:
                    {
                        if (expression is BinaryExpression { Left: MemberExpression memberExpr, Right: ConstantExpression constExpr } binaryExpr)
                        {
                            var op = binaryExpr.NodeType switch
                            {
                                ExpressionType.Equal => "=",
                                ExpressionType.GreaterThan => ">",
                                ExpressionType.LessThan => "<",
                                _ => throw new NotSupportedException($"عملگر {binaryExpr.NodeType} پشتیبانی نمی‌شود.")
                            };

                            var paramName = $"@p{paramCounter++}";
                            sqlBuilder.Append($"{memberExpr.Member.Name} {op} {paramName}");
                            parameters.Add(paramName, constExpr.Value);
                        }
                        else
                        {
                            throw new NotSupportedException($"نوع عبارت {expression.NodeType} پشتیبانی نمی‌شود.");
                        }

                        break;
                    }
            }
        }
    }

    private void EnsureConnectionOpen()
    {
        if (dbConnection.State != ConnectionState.Open)
            dbConnection.Open();
    }

    private void CloseConnection()
    {
        if (dbConnection.State == ConnectionState.Open)
            dbConnection.Close();
    }


    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        try
        {
            EnsureConnectionOpen();
            var result = await dbConnection.QueryAsync<TEntity>($"SELECT * FROM {EscapeTableName(_tableName)}");

            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"خطا در دریافت همه رکوردهای {typeof(TEntity).Name}.", ex);
        }
        finally
        {
            CloseConnection();
        }
    }

    public async Task<IEnumerable<TEntity>> GetPagedAsync(int page, int pageSize, string orderBy = nameof(Aggregate.Id))
    {
        try
        {
            if (page < 1 || pageSize < 1)
                throw new ArgumentException("صفحه و اندازه صفحه باید مثبت باشند.");

            var sql = $"SELECT * FROM {EscapeTableName(_tableName)} ORDER BY {EscapeTableName(orderBy)} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            EnsureConnectionOpen();
            return await dbConnection.QueryAsync<TEntity>(sql, new { Offset = (page - 1) * pageSize, PageSize = pageSize });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"خطا در دریافت همه رکوردهای {typeof(TEntity).Name}.", ex);
        }
        finally
        {
            CloseConnection();
        }
    }

    public async Task<TEntity?> GetByIdAsync(long id)
    {
        try
        {
            EnsureConnectionOpen();
            return await dbConnection.QueryFirstOrDefaultAsync<TEntity>($"SELECT * FROM {EscapeTableName(_tableName)} WHERE Id = @Id", new { Id = id });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"خطا در دریافت {typeof(TEntity).Name} با شناسه {id}.", ex);
        }
        finally
        {
            CloseConnection();
        }
    }

    public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        try
        {
            var (sqlWhere, parameters) = BuildWhereClause(predicate);
            var sql = $"SELECT * FROM {EscapeTableName(_tableName)} WHERE {sqlWhere}";

            EnsureConnectionOpen();
            return await dbConnection.QueryAsync<TEntity>(sql, parameters);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"خطا در دریافت همه رکوردهای {typeof(TEntity).Name}.", ex);
        }
        finally
        {
            CloseConnection();
        }
    }

    public async Task<int> ExecuteRawQueryAsync(string sql, object? parameters = null)
    {
        try
        {
            EnsureConnectionOpen();
            return await dbConnection.ExecuteAsync(sql, parameters);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("خطا در اجرای درخواست", ex);
        }
        finally
        {
            CloseConnection();
        }
    }

    public async Task<IEnumerable<T>> ExecuteJoinQueryAsync<T>(string joinSql, object? parameters = null)
    {
        try
        {
            EnsureConnectionOpen();
            return await dbConnection.QueryAsync<T>($"SELECT * FROM {EscapeTableName(_tableName)} {joinSql}", parameters);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"خطا در اجرای کوئری join برای {typeof(T).Name}.", ex);
        }
        finally
        {
            CloseConnection();
        }
    }

    public async Task<IEnumerable<T>> ExecuteSPAsync<T>(string procedureName, object? parameters = null)
    {
        try
        {
            EnsureConnectionOpen();
            return await dbConnection.QueryAsync<T>(
                procedureName,
                parameters,
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"خطا در اجرای درخواست {procedureName} برای {typeof(T).Name}.", ex);
        }
        finally
        {
            CloseConnection();
        }
    }

    public async Task<T?> ExecuteSPSingleAsync<T>(string procedureName, object? parameters = null)
    {
        try
        {
            EnsureConnectionOpen();
            return await dbConnection.QueryFirstOrDefaultAsync<T>(
                procedureName,
                parameters,
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"خطا در اجرای درخواست {procedureName} برای تک {typeof(T).Name}.", ex);
        }
        finally
        {
            CloseConnection();
        }
    }

    public async Task<int> ExecuteSPNonQueryAsync(string procedureName, object? parameters = null)
    {
        try
        {
            EnsureConnectionOpen();
            return await dbConnection.ExecuteAsync(
                procedureName,
                parameters,
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"خطا در اجرای درخواست {procedureName}.", ex);
        }
        finally
        {
            CloseConnection();
        }
    }

    public async Task<(T Result, Dictionary<string, object> OutputParams)> ExecuteSPWithOutputsAsync<T>(
        string procedureName, object inputParams, Dictionary<string, DbType> outputParams)
    {
        try
        {
            var parameters = new DynamicParameters(inputParams);
            foreach (var param in outputParams)
            {
                parameters.Add(param.Key, dbType: param.Value, direction: ParameterDirection.Output);
            }

            EnsureConnectionOpen();
            var result = await dbConnection.QueryFirstOrDefaultAsync<T>(procedureName, parameters, commandType: CommandType.StoredProcedure);

            var outputValues = new Dictionary<string, object>();
            foreach (var param in outputParams)
            {
                outputValues[param.Key] = parameters.Get<object>(param.Key);
            }

            return (result, outputValues)!;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"خطا در اجرای درخواست {procedureName}.", ex);
        }
        finally
        {
            CloseConnection();
        }
    }
}