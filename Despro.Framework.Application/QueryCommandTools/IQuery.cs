using Despro.Framework.Base.BaseModels;
using Despro.Framework.Base.BaseModels.GridData;
using MediatR;

namespace Despro.Framework.Application.QueryCommandTools;

public interface IQueryOperation<TResponse> : IRequest<OperationQueryResult<TResponse>>;
public interface IQuery<out TResponse> : IRequest<TResponse>;

public abstract class QueryGrid<TResponse>(BaseGrid baseGrid) : IRequest<GridData<TResponse>>
    where TResponse : BaseDto
{
    public BaseGrid BaseGrid { get; private set; } = baseGrid;
}

public abstract class QueryGridOperation<TResponse>(BaseGrid baseGrid) : IRequest<OperationQueryResult<GridData<TResponse>>>
    where TResponse : BaseDto
{
    public BaseGrid BaseGrid { get; private set; } = baseGrid;
}



public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>;

public interface IQueryGridHandler<in TQuery, TResponse> : IRequestHandler<TQuery, GridData<TResponse>>
    where TResponse : BaseDto
    where TQuery : QueryGrid<TResponse>;

public interface IQueryOperationHandler<in TQuery, TResponse> : IRequestHandler<TQuery, OperationQueryResult<TResponse>>
    where TQuery : IQueryOperation<TResponse>;

public interface IQueryGridOperationHandler<in TQuery, TResponse> : IRequestHandler<TQuery, OperationQueryResult<GridData<TResponse>>>
    where TResponse : BaseDto
    where TQuery : QueryGridOperation<TResponse>;