using Despro.Framework.Base.BaseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Despro.Framework.Base.BaseExtensions;

public static class IncludeExtensions
{
    #region IncludeFiltered
    public static IIncludableQueryable<TEntity, TProperty> IncludeFiltered<TEntity, TProperty, TId>(
        this IQueryable<TEntity> query,
        Expression<Func<TEntity, TProperty>> navigationExpression)
        where TEntity : Aggregate<TId>
        where TProperty : Aggregate<TId>
        where TId : notnull
    {
        return query.Include(navigationExpression);
    }
    public static IIncludableQueryable<TEntity, TProperty> IncludeFiltered<TEntity, TProperty, TId>(
        this IQueryable<TEntity> query,
        Expression<Func<TEntity, TProperty>> navigationExpression,
        bool withoutDeleted = false)
        where TEntity : Aggregate<TId>
        where TProperty : Aggregate<TId>
        where TId : notnull
    {
        return query.Include(navigationExpression);
    }

    public static IIncludableQueryable<TEntity, IEnumerable<TProperty>> IncludeFiltered<TEntity, TProperty, TId>(
        this IQueryable<TEntity> query,
        Expression<Func<TEntity, IEnumerable<TProperty>>> navigationExpression,
        bool withoutDeleted = false)
        where TEntity : Aggregate<TId>
        where TProperty : Aggregate<TId>
        where TId : notnull
    {
        if (withoutDeleted)
            return query.Include(navigationExpression);

        var filterTemplate = (Expression<Func<IEnumerable<TProperty>, IEnumerable<TProperty>>>)(q => q.Where(e => !e.IsDelete));
        var filterBody = ReplacingExpressionVisitor.Replace(filterTemplate.Parameters[0], navigationExpression.Body, filterTemplate.Body);
        var filterLambda = Expression.Lambda<Func<TEntity, IEnumerable<TProperty>>>(filterBody, navigationExpression.Parameters);

        return query.Include(filterLambda);
    }
    #endregion

    #region ThenIncludeFiltered
    public static IIncludableQueryable<TEntity, TProperty> ThenIncludeFiltered<TEntity, TPreviousProperty, TProperty, TId>(
        this IIncludableQueryable<TEntity, TPreviousProperty> query,
        Expression<Func<TPreviousProperty, TProperty>> navigationExpression,
        bool withoutDeleted = false)
        where TEntity : Aggregate<TId>
        where TProperty : Aggregate<TId>
        where TId : notnull
    {
        return query.ThenInclude(navigationExpression);
    }

    public static IIncludableQueryable<TEntity, TProperty> ThenIncludeFiltered<TEntity, TPreviousProperty, TProperty, TId>(
        this IIncludableQueryable<TEntity, IEnumerable<TPreviousProperty>> query,
        Expression<Func<TPreviousProperty, TProperty>> navigationExpression,
        bool withoutDeleted = false)
        where TEntity : Aggregate<TId>
        where TProperty : Aggregate<TId>
        where TId : notnull
    {
        return query.ThenInclude(navigationExpression);
    }

    public static IIncludableQueryable<TEntity, IEnumerable<TProperty>> ThenIncludeFiltered<TEntity, TPreviousProperty, TProperty, TId>(
        this IIncludableQueryable<TEntity, IEnumerable<TPreviousProperty>> query,
        Expression<Func<TPreviousProperty, IEnumerable<TProperty>>> navigationExpression,
        bool withoutDeleted = false)
        where TEntity : Aggregate<TId>
        where TProperty : Aggregate<TId>
        where TId : notnull
    {
        if (withoutDeleted)
            return query.ThenInclude(navigationExpression);

        var filterTemplate = (Expression<Func<IEnumerable<TProperty>, IEnumerable<TProperty>>>)(q => q.Where(e => !e.IsDelete));
        var filterBody = ReplacingExpressionVisitor.Replace(filterTemplate.Parameters[0], navigationExpression.Body, filterTemplate.Body);
        var filterLambda = Expression.Lambda<Func<TPreviousProperty, IEnumerable<TProperty>>>(filterBody, navigationExpression.Parameters);

        return query.ThenInclude(filterLambda);
    }

    public static IIncludableQueryable<TEntity, IEnumerable<TProperty>> ThenIncludeFiltered<TEntity, TPreviousProperty, TProperty, TId>(
        this IIncludableQueryable<TEntity, TPreviousProperty> query,
        Expression<Func<TPreviousProperty, IEnumerable<TProperty>>> navigationExpression,
        bool withoutDeleted = false)
        where TEntity : Aggregate<TId>
        where TProperty : Aggregate<TId>
        where TId : notnull
    {
        if (withoutDeleted)
            return query.ThenInclude(navigationExpression);

        var filterTemplate = (Expression<Func<IEnumerable<TProperty>, IEnumerable<TProperty>>>)(q => q.Where(e => !e.IsDelete));
        var filterBody = ReplacingExpressionVisitor.Replace(filterTemplate.Parameters[0], navigationExpression.Body, filterTemplate.Body);
        var filterLambda = Expression.Lambda<Func<TPreviousProperty, IEnumerable<TProperty>>>(filterBody, navigationExpression.Parameters);

        return query.ThenInclude(filterLambda);
    }
    #endregion
}