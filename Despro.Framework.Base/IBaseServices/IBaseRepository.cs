using Despro.Framework.Base.BaseModels;

namespace Despro.Framework.Base.IBaseServices;

public interface IBaseRepository<TEntity, in TId> :
    IBasePublisherRepository<TEntity, TId>,
    IBaseReadRepository<TEntity, TId>
    where TEntity : Aggregate<TId>
    where TId : notnull;