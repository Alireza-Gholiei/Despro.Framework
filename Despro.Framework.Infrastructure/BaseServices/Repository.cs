using Despro.Framework.Base.BaseModels;
using Despro.Framework.Infrastructure.BaseServices.IDIContainer;
using Despro.Framework.Infrastructure.Contexts;

namespace Despro.Framework.Infrastructure.BaseServices;

internal class Repository<TContext, TEntity, TId>(TContext context, IRepositoryServices repositoryServices)
    : BaseRepository<TContext, TEntity, TId>(context, repositoryServices)
    where TContext : EfBaseContext
    where TEntity : Aggregate<TId>
    where TId : notnull;