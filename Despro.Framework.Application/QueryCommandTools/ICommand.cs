using Despro.Framework.Base.BaseModels;
using Despro.Framework.Base.IMediator;

namespace Despro.Framework.Application.QueryCommandTools;

public interface ICommand : IRequest<OperationResult>;

public interface ICommand<TResponse> : IRequest<OperationResult<TResponse>>;



public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, OperationResult>
    where TCommand : ICommand;

public interface ICommandHandler<in TCommand, TResponseData> : IRequestHandler<TCommand, OperationResult<TResponseData>>
    where TCommand : ICommand<TResponseData>;