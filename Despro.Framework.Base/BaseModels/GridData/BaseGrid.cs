using Newtonsoft.Json;

namespace Despro.Framework.Base.BaseModels.GridData;

public class FilterParam(string key, string value)
{
    public void SetOperator(string @operator)
    {
        Operator = @operator;
    }

    public void SetExpression(bool searchExpression)
    {
        SearchExpression = searchExpression;
    }

    public void SetOperatorExpression(string @operator, bool searchExpression)
    {
        Operator = @operator;
        SearchExpression = searchExpression;
    }

    public string Key { get; private set; } = key;
    public string Value { get; private set; } = value;
    public string? Operator { get; private set; }
    public bool SearchExpression { get; private set; }
}

public enum OrderType
{
    Ascending,
    Descending,
}

public class BaseGrid
{
    public int CurrentPage { get; set; }
    public int Limit { get; set; }
    public string OrderField { get; set; }
    public OrderType OrderType { get; set; }
    public List<FilterParam> FilterParam { get; set; } = new List<FilterParam>();

    public void Set(int currentPage, int limit, string orderField, OrderType orderType, List<FilterParam> filterParam)
    {
        CurrentPage = currentPage;
        Limit = limit;
        OrderField = orderField;
        OrderType = orderType;
        FilterParam = filterParam;
    }

    public void Set(string grid)
    {
        if (!string.IsNullOrEmpty(grid) && grid != "null")
        {
            var baseGrid = JsonConvert.DeserializeObject<BaseGrid>(grid)!;
            CurrentPage = baseGrid.CurrentPage;
            Limit = baseGrid.Limit;
            OrderField = baseGrid.OrderField;
            OrderType = baseGrid.OrderType;
            FilterParam = baseGrid.FilterParam;
        }
        else
        {
            CurrentPage = 1;
            Limit = 10;
            OrderField = "Id";
            OrderType = OrderType.Descending;
            FilterParam = new List<FilterParam>();
        }
    }
}

public class GridData<TData> : BaseGrid
{
    public GridData(List<TData> data, BaseGrid baseGrid, int entityCount)
    {
        GeneratePaging(data, entityCount, baseGrid);
    }

    public GridData(IQueryable<TData> data, BaseGrid baseGrid, int entityCount)
    {
        GeneratePaging(data, entityCount, baseGrid);
    }

    public GridData(IEnumerable<TData> data, BaseGrid baseGrid, int entityCount)
    {
        GeneratePaging(data, entityCount, baseGrid);
    }

    public List<TData> Data { get; private set; }
    public int EntityCount { get; private set; }
    public int PageCount { get; private set; }

    public void GeneratePaging(IQueryable<TData> data, int count, BaseGrid baseGrid)
    {
        Data = data.ToList();
        var entityCount = count;
        var pageCount = (int)Math.Ceiling(entityCount / (double)baseGrid.Limit);
        PageCount = pageCount;
        EntityCount = entityCount;
        Set(baseGrid.CurrentPage, baseGrid.Limit, baseGrid.OrderField, baseGrid.OrderType, baseGrid.FilterParam);
    }

    public void GeneratePaging(IEnumerable<TData> data, int count, BaseGrid baseGrid)
    {
        Data = data.ToList();
        var entityCount = count;
        var pageCount = (int)Math.Ceiling(entityCount / (double)baseGrid.Limit);
        PageCount = pageCount;
        EntityCount = entityCount;
        Set(baseGrid.CurrentPage, baseGrid.Limit, baseGrid.OrderField, baseGrid.OrderType, baseGrid.FilterParam);
    }

    public void GeneratePaging(List<TData> data, int count, BaseGrid baseGrid)
    {
        Data = data;
        var entityCount = count;
        var pageCount = (int)Math.Ceiling(entityCount / (double)baseGrid.Limit);
        PageCount = pageCount;
        EntityCount = entityCount;
        Set(baseGrid.CurrentPage, baseGrid.Limit, baseGrid.OrderField, baseGrid.OrderType, baseGrid.FilterParam);
    }

    public void GeneratePaging(int count, BaseGrid baseGrid)
    {
        var entityCount = count;
        var pageCount = (int)Math.Ceiling(entityCount / (double)baseGrid.Limit);
        PageCount = pageCount;
        EntityCount = entityCount;
        Set(baseGrid.CurrentPage, baseGrid.Limit, baseGrid.OrderField, baseGrid.OrderType, baseGrid.FilterParam);
    }
}