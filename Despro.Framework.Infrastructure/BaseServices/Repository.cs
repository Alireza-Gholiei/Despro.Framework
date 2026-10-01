using Despro.Framework.Base.BaseModels;
using Despro.Framework.Infrastructure.BaseServices.IDIContainer;
using Despro.Framework.Infrastructure.Contexts;

namespace Despro.Framework.Infrastructure.BaseServices;

internal class Repository<TEntity, TId>(EfBaseContext context, IRepositoryServices repositoryServices)
    : BaseRepository<EfBaseContext, TEntity, TId>(context, repositoryServices)
    where TEntity : Aggregate<TId>
    where TId : notnull;