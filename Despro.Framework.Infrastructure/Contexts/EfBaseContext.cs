using Despro.Framework.Base.BaseModels;
using Despro.Framework.Infrastructure.Mediator;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace Despro.Framework.Infrastructure.Contexts;

public abstract class EfBaseContext(
    DbContextOptions options,
    ICustomPublisher publisher,
    Assembly configurationsAssembly)
    : DbContext(options)
{
    private readonly ICustomPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    private readonly Assembly _configurationsAssembly = configurationsAssembly ?? throw new ArgumentNullException(nameof(configurationsAssembly));

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        var aggregates = GetAggregatesWithEvents();

        var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

        await PublishEvents(aggregates, cancellationToken);

        return result;
    }

    private List<IAggregateRoot> GetAggregatesWithEvents()
    {
        return ChangeTracker.Entries<IAggregateRoot>()
            .Where(x => x.State != EntityState.Detached)
            .Select(x => x.Entity)
            .Where(x => x.DomainEvents.Count > 0)
            .ToList();
    }

    private async Task PublishEvents(List<IAggregateRoot> aggregates, CancellationToken cancellationToken)
    {
        if (aggregates.Count == 0) return;

        var events = aggregates.SelectMany(a => a.DomainEvents).ToList();

        foreach (var aggregate in aggregates)
            aggregate.ClearDomainEvents();

        foreach (var domainEvent in events)
            await _publisher.Publish(domainEvent, PublishStrategy.Async, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        foreach (var fk in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            if (!fk.IsOwnership)
                fk.DeleteBehavior = DeleteBehavior.Restrict;
        }

        builder.ApplyConfigurationsFromAssembly(_configurationsAssembly);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            // QueryFilter فقط روی root entity مجازه
            if (entityType.BaseType is not null || entityType.IsOwned()) continue;
            if (!IsAggregate(entityType.ClrType)) continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var isDelete = Expression.Property(parameter, "IsDelete");
            var notDeleted = Expression.Equal(isDelete, Expression.Constant(false));

            builder.Entity(entityType.ClrType).HasQueryFilter(Expression.Lambda(notDeleted, parameter));
        }
    }

    private static bool IsAggregate(Type type)
    {
        for (var t = type; t is not null; t = t.BaseType)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Aggregate<>))
                return true;
        }

        return false;
    }

    public DbSet<SystemError> SystemError { get; set; }
}