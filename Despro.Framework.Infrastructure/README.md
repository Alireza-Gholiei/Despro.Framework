# Despro.Framework.Infrastructure

> The **infrastructure** layer of [Despro Framework](../README.md).

`Despro.Framework.Infrastructure` provides the concrete implementations of the contracts
declared in `Despro.Framework.Base`: EF Core + Dapper data access, the Unit of Work,
JWT-claim-based authentication, error logging, deferred MongoDB audit logging, and a custom
MediatR notification dispatcher for domain events. It is the composition root that wires
these services into the DI container.

- **Package:** `Despro.Framework.Infrastructure`
- **Version:** `2.1.3`
- **Target framework:** `net10.0`
- **Depends on:** `Despro.Framework.Base`

---

## Installation

```bash
dotnet add package Despro.Framework.Infrastructure
```

### DI registration

```csharp
using System.Reflection;
using Despro.Framework.Infrastructure;

builder.Services.AddFrameworkInfrastructure(
    builder.Configuration,
    useCaseAssembly: Assembly.GetExecutingAssembly(), // assembly containing your commands
    queryAssembly:   Assembly.GetExecutingAssembly(), // assembly containing your queries
    MongoDbLog: true);                                // false → no-op logging
```

`AddFrameworkInfrastructure` registers:

- `IAuthService → AuthService`, `IErrorLogger → ErrorLogger`
- `IUnitOfWork → UnitOfWork`, `IBaseRepository<> → Repository<>`, `IRepositoryServices → RepositoryServices`
- `ICustomPublisher → CustomPublisher`
- MediatR (scanning `CustomPublisher`'s assembly plus your `useCaseAssembly` / `queryAssembly`) and their FluentValidation validators
- **When `MongoDbLog: true`:** `IMongoClient`/`IMongoDatabase` (from `MongoDbConfig:ConnectionString` — throws if missing), `ILogService → MongoLogService`, `ILoggingContext → LoggingContext`
- **When `MongoDbLog: false`:** `ILogService → NullLogService`, `ILoggingContext → NullLoggingContext`

---

## Key components

### Data access (`BaseServices`, `Contexts`)

| Type | Role |
| --- | --- |
| `EfBaseContext` | Abstract `DbContext` base. Applies a global soft-delete query filter (`!IsDelete`) to every `BaseEntity`, forces `DeleteBehavior.Restrict` on non-owned FKs, applies `IEntityTypeConfiguration` from a supplied assembly, names tables by CLR type, and **dispatches queued domain events** on `SaveChangesAsync`. Exposes `DbSet<SystemError>`. |
| `BaseRepository<TEntity>` / `Repository<TEntity>` | Generic EF Core repository (`IBaseRepository<>`). Applies audit stamps automatically and queues an audit log entry on write. |
| `DapperRepository<TEntity>` | `IDapperRepository<>` — raw SQL, expression-to-WHERE translation, paging, stored procedures (including output params). |
| `DapperContext` | Creates `SqlConnection`s for Dapper. |
| `UnitOfWork` | `IUnitOfWork` over `EfBaseContext`; flushes pending audit logs after `SaveChanges(Async)` and manages transactions (`ExecuteTransactionAsync`, …). |
| `RepositoryServices` | Aggregates `IAuthService`, `ILoggingContext` and `IServiceProvider` for the repositories. |

### Authentication (`BaseServices/AuthService`)

`AuthService` implements `IAuthService` by reading the current `HttpContext` user's JWT
claims (user id, full name, roles, expiry, IP) and provides role-validation guards
(`CheckRoleValidation`, `CheckRoleValidationByRoleGuid`) that throw `AuthException` on
mismatch.

### Error logging (`BaseServices/ErrorLogger`)

`ErrorLogger` implements `IErrorLogger`, serializing exception details (with the acting
user and Persian date) to per-day JSON files and building a `SystemError` record.

### Audit logging (`InfrastructureServices`, `InfrastructureModels`)

- `LoggingContext` (`ILoggingContext`) buffers log entries in memory and flushes them in
  batches (`BatchSize = 100`) to `ILogService`.
- `MongoLogService` (`ILogService`) persists `LogEntity` documents to the `LogEntries`
  MongoDB collection and provisions indexes on entity name / date / user / log type.
- `NullLogService` / `NullLoggingContext` are the no-op implementations used when MongoDB
  logging is disabled, so calling code never branches on whether logging is on.
- `LogEntity` + `OperationLogType` (`Add`/`Update`/`Delete`) model the audit records.

### Domain-event dispatch (`MediatR`)

- `ICustomPublisher` / `CustomPublisher` — publishes MediatR notifications with a selectable
  `PublishStrategy` (`SyncContinueOnException`, `SyncStopOnException`, `Async`,
  `ParallelNoWait`, `ParallelWhenAll`, `ParallelWhenAny`).
- `CustomMediator` — a `Mediator` subclass overriding `PublishCore` to apply the chosen
  strategy. `EfBaseContext` uses `PublishStrategy.Async` to flush `AggregateRoot` domain
  events during `SaveChangesAsync`.

### Exceptions (`InfrastructureExceptions`)

`BaseInfrastructureException` (abstract), `AuthException`, `BaseRepositoryException`.

---

## Configuration

| Key | When | Notes |
| --- | --- | --- |
| `MongoDbConfig:ConnectionString` | `MongoDbLog: true` | Required; throws if missing. |
| `MongoDbConfig:DatabaseName` | `MongoDbLog: true` | Audit-log database. |

---

## Usage

Derive your application's `DbContext` from `EfBaseContext`:

```csharp
public class AppDbContext(
    DbContextOptions<AppDbContext> options,
    ICustomPublisher publisher)
    : EfBaseContext(options, publisher, typeof(AppDbContext).Assembly)
{
    public DbSet<Product> Products => Set<Product>();
}
```

Then inject `IBaseRepository<Product>` and `IUnitOfWork` into your handlers — audit stamps,
soft-delete filtering, audit logging and domain-event dispatch are handled for you.

---

## Project structure

```
Despro.Framework.Infrastructure
├── BaseServices/            # AuthService, Repository, UnitOfWork, ErrorLogger
│   ├── DbServices/          # DapperRepository
│   ├── DIContainer/         # RepositoryServices
│   └── IDIContainer/        # IRepositoryServices
├── Contexts/                # EfBaseContext, DapperContext
├── InfrastructureExceptions/# AuthException, BaseRepositoryException
├── InfrastructureIServices/ # ILogService, ILoggingContext
├── InfrastructureModels/    # LogEntity, OperationLogType
├── InfrastructureServices/  # LoggingContext, MongoLogService, Null* (no-op)
├── MediatR/                 # ICustomPublisher, CustomPublisher, CustomMediator, PublishStrategy
└── FrameworkInfrastructureDi.cs # AddFrameworkInfrastructure
```
