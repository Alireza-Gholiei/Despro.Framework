using Despro.Framework.Base.BaseModels;
using Despro.Framework.Infrastructure.BaseServices.IDIContainer;
using Despro.Framework.Infrastructure.Contexts;

namespace Despro.Framework.Infrastructure.BaseServices;

internal class Repository<TEntity>(EfBaseContext context, IRepositoryServices repositoryServices)
    : BaseRepository<TEntity>(context, repositoryServices)
    where TEntity : BaseEntity;