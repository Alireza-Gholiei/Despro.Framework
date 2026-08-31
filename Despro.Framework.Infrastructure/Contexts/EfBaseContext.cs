using Despro.Framework.Base.BaseModels;
using Despro.Framework.Infrastructure.MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace Despro.Framework.Infrastructure.Contexts;

public abstract class EfBaseContext : DbContext
{
    private readonly ICustomPublisher _publisher;
    private readonly Assembly _configurationsAssembly;

    /// <summary>
    /// Base Db Context
    /// </summary>
    /// <param name="options"></param>
    /// <param name="publisher"></param>
    /// <param name="configurationsAssembly">Assembly IEntityTypeConfiguration</param>
    protected EfBaseContext(DbContextOptions options, ICustomPublisher publisher, Assembly configurationsAssembly) : base(options)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _configurationsAssembly = configurationsAssembly ?? throw new ArgumentNullException(nameof(configurationsAssembly));
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var modifiedEntities = GetModifiedEntities();

        await PublishEvents(modifiedEntities, cancellationToken);

        return await base.SaveChangesAsync(cancellationToken);
    }

    private List<AggregateRoot> GetModifiedEntities()
    {
        return ChangeTracker.Entries<AggregateRoot>()
            .Where(x => x.State != EntityState.Detached)
            .Select(c => c.Entity)
            .Where(c => c.DomainEvents.Any())
            .ToList();
    }

    private async Task PublishEvents(List<AggregateRoot> modifiedEntities, CancellationToken cancellationToken)
    {
        if (modifiedEntities?.Any() != true) return;

        foreach (var entity in modifiedEntities)
        {
            List<IDomainEvent> events = [.. entity.DomainEvents];

            foreach (var domainEvent in events)
            {
                entity.DomainEvents.Remove(domainEvent);
                await _publisher.Publish(domainEvent, PublishStrategy.Async, cancellationToken);
            }
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        foreach (var fk in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            if (!fk.IsOwnership)
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        builder.ApplyConfigurationsFromAssembly(_configurationsAssembly);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (!typeof(Aggregate).IsAssignableFrom(entityType.ClrType)) continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");

            var isDeleteProperty = Expression.Property(parameter, nameof(Aggregate.IsDelete));
            var notDeleted = Expression.Equal(isDeleteProperty, Expression.Constant(false));

            var lambda = Expression.Lambda(notDeleted, parameter);

            builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }


    public DbSet<SystemError> SystemError { get; set; }
}