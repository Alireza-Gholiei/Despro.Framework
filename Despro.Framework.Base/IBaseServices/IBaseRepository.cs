using Despro.Framework.Base.BaseModels;

namespace Despro.Framework.Base.IBaseServices;

public interface IBaseRepository<TEntity> : IBasePublisherRepository<TEntity>, IBaseReadRepository<TEntity>
    where TEntity : BaseEntity;