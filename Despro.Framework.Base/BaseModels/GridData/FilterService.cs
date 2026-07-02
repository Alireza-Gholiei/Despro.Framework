using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace Despro.Framework.Base.BaseModels.GridData;

public static class FilterService
{
    #region Paging & Ordering

    extension<T>(IQueryable<T> query)
    {
        public IQueryable<T> PagingList(BaseGrid baseGrid, bool applyOrder = true)
        {
            var skip = (baseGrid.CurrentPage - 1) * baseGrid.Limit;

            if (string.IsNullOrWhiteSpace(baseGrid.OrderField))
                return query.Skip(skip).Take(baseGrid.Limit);

            query = query.OrderList(baseGrid, applyOrder);

            return query.Skip(skip).Take(baseGrid.Limit);
        }

        public IQueryable<T> FilterList(BaseGrid baseGrid, bool applyOrder = true)
        {
            foreach (var filter in baseGrid.FilterParam)
            {
                var expr = TranslateFilter<T>(filter.Key, filter.Value);
                if (expr != null)
                    query = query.Where(expr);
            }

            query = query.OrderList(baseGrid, applyOrder);

            return query;
        }

        public IQueryable<T> FilterPagingList(BaseGrid baseGrid, bool applyOrder = true)
        {
            var skip = (baseGrid.CurrentPage - 1) * baseGrid.Limit;

            foreach (var filter in baseGrid.FilterParam)
            {
                var expr = TranslateFilter<T>(filter.Key, filter.Value);
                if (expr != null)
                    query = query.Where(expr);
            }

            query = query.OrderList(baseGrid, applyOrder);

            return query.Skip(skip).Take(baseGrid.Limit);
        }

        private IQueryable<T> OrderList(BaseGrid baseGrid, bool applyOrder = true)
        {
            if (!applyOrder || string.IsNullOrWhiteSpace(baseGrid.OrderField))
                return query;

            var propInfo = GetPropertyRecursive(typeof(T), baseGrid.OrderField);
            if (propInfo == null)
                return query;

            var orderProperty = GetGetter<T, object>(propInfo);
            query = baseGrid.OrderType == OrderType.Ascending
                ? query.OrderBy(orderProperty)
                : query.OrderByDescending(orderProperty);

            return query;
        }
    }



    #endregion

    #region Filter Translation
    private static Expression<Func<T, bool>> TranslateFilter<T>(string propertyPath, string value)
    {
        var param = Expression.Parameter(typeof(T), "e");
        var body = BuildExpressionForPath(param, typeof(T), propertyPath, value);

        return body == null ? null : Expression.Lambda<Func<T, bool>>(body, param);
    }

    private static Expression? BuildExpressionForPath(ParameterExpression param, Type currentType, string propertyPath, string value)
    {
        var parts = propertyPath.Split('.');
        Expression body = param;

        for (var i = 0; i < parts.Length; i++)
        {
            var propInfo = currentType.GetProperty(parts[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (propInfo == null)
                return null;

            currentType = propInfo.PropertyType;

            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(currentType) && currentType != typeof(string))
            {
                var itemType = currentType.GetGenericArguments()[0];
                var itemParam = Expression.Parameter(itemType, "x");

                var remainingPath = string.Join('.', parts[(i + 1)..]);
                var itemExpr = BuildExpressionForPath(itemParam, itemType, remainingPath, value);

                if (itemExpr == null)
                    return null;

                var anyCall = Expression.Call(
                    typeof(Enumerable),
                    "Any",
                    [itemType],
                    Expression.Property(body, propInfo),
                    Expression.Lambda(itemExpr, itemParam)
                );

                return anyCall;
            }

            body = Expression.Property(body, propInfo);
        }

        return body.Type switch
        {
            { } t when t == typeof(string) => StringContainsIgnoreCaseExpression(body, value),
            { } t when t.BaseType == typeof(Enum) => EnumContainsExpression(body, value),
            { } t when t == typeof(long) || t == typeof(int) => NumberEqualsExpression(body, value),
            { } t when t == typeof(decimal) => DecimalEqualsExpression(body, value),
            { } t when t == typeof(bool) => BoolEqualsExpression(body, value),
            { } t when t == typeof(DateTime) => DateEqualsExpression(body, value),
            _ => null
        };
    }
    #endregion

    #region ExpressionBuilders
    private static Expression StringContainsIgnoreCaseExpression(Expression prop, string value)
    {
        var efFunctions = Expression.Property(null, typeof(EF).GetProperty(nameof(EF.Functions)));
        var likeMethod = typeof(DbFunctionsExtensions).GetMethod(
            nameof(DbFunctionsExtensions.Like),
            [typeof(DbFunctions), typeof(string), typeof(string)]);

        var pattern = Expression.Constant($"%{value}%");
        var call = Expression.Call(null, likeMethod, efFunctions, prop, pattern);

        return call;
    }

    private static Expression EnumContainsExpression(Expression prop, string value)
    {
        var toStringCall = Expression.Call(prop, prop.Type.GetMethod("ToString", Type.EmptyTypes));

        var efFunctions = Expression.Property(null, typeof(EF).GetProperty(nameof(EF.Functions)));
        var likeMethod = typeof(DbFunctionsExtensions).GetMethod(
            nameof(DbFunctionsExtensions.Like),
            [typeof(DbFunctions), typeof(string), typeof(string)]);

        var pattern = Expression.Constant($"%{value}%");
        var call = Expression.Call(null, likeMethod, efFunctions, toStringCall, pattern);

        return call;
    }

    private static Expression NumberEqualsExpression(Expression prop, string value)
    {
        var converted = Convert.ChangeType(value, prop.Type);
        var constant = Expression.Constant(converted, prop.Type);
        return Expression.Equal(prop, constant);
    }

    private static Expression DecimalEqualsExpression(Expression prop, string value)
    {
        if (!decimal.TryParse(value, out var decValue))
            return null;

        var constant = Expression.Constant(decValue);
        return Expression.Equal(prop, constant);
    }

    private static Expression BoolEqualsExpression(Expression prop, string value)
    {
        if (!bool.TryParse(value, out var boolValue))
            return null;

        var constant = Expression.Constant(boolValue);
        return Expression.Equal(prop, constant);
    }

    private static Expression DateEqualsExpression(Expression prop, string value)
    {
        if (!DateTime.TryParse(value, out var dateValue))
            return null;

        var constant = Expression.Constant(dateValue);
        return Expression.Equal(prop, constant);
    }
    #endregion

    #region Helpers
    private static Expression<Func<T, P>> GetGetter<T, P>(PropertyInfo propInfo)
    {
        var param = Expression.Parameter(typeof(T), "e");
        var body = Expression.Property(param, propInfo);
        var converted = Expression.Convert(body, typeof(object));
        return Expression.Lambda<Func<T, P>>(converted, param);
    }

    private static PropertyInfo GetPropertyRecursive(Type type, string propertyPath)
    {
        var parts = propertyPath.Split('.');
        var currentType = type;
        PropertyInfo propInfo = null;

        foreach (var part in parts)
        {
            propInfo = currentType.GetProperty(part, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (propInfo == null) return null;

            currentType = propInfo.PropertyType;
            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(currentType) && currentType.IsGenericType)
                currentType = currentType.GetGenericArguments()[0];
        }

        return propInfo;
    }

    #endregion
}