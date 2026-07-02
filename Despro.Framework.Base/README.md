# Despro.Framework.Base

> The **core abstractions** layer of [Despro Framework](../README.md).

`Despro.Framework.Base` is the innermost package of the framework's Clean Architecture. It
holds the contracts, base models and cross-cutting utilities that **every other layer
depends on** and defines *no* concrete infrastructure of its own — the higher layers
(Infrastructure, Presentation, WebClient, …) implement the interfaces declared here.

- **Package:** `Despro.Framework.Base`
- **Version:** `2.1.0`
- **Target framework:** `net10.0`

```
        ┌──────────────────────────── everything depends on Base ────────────────────────────┐
Base ◄── Domain   Application   Infrastructure   Presentation   WebClient
```

---

## Installation

```bash
dotnet add package Despro.Framework.Base
```

Base is pulled in transitively by every other Despro package, so you rarely install it
directly unless you are authoring shared contracts/DTOs.

### Dependencies

| Package | Version | Purpose |
| --- | --- | --- |
| MediatR | 14.0.0 | `INotification` base for domain events |
| FluentValidation | 12.1.1 | Base validators (`FileValidator`) |
| FluentValidation.DependencyInjectionExtensions | 12.1.1 | Validator registration helpers |
| Mapster | 7.4.0 | DTO projection support |
| Microsoft.EntityFrameworkCore | 10.0.2 | `IQueryable` filtering / include helpers (`PrivateAssets`) |
| Newtonsoft.Json | 13.0.4 | `BaseGrid` (de)serialization |

---

## What's inside

### Repository & Unit of Work contracts (`IBaseServices`)

The persistence abstractions implemented by `Despro.Framework.Infrastructure`.

| Contract | Responsibility |
| --- | --- |
| `IBaseReadRepository<TEntity>` | Read side: `GetByIdAsync`, `GetAllAsync`, `AnyAsync`, `CountAsync`, `Table()`, paging via `GetFilterPaging` / `GetFilterPagingDtoAsync<TDto>`. |
| `IBasePublisherRepository<TEntity>` | Write side: `AddAsync`, `UpdateAsync`, `UpdatePartialAsync`, soft `RemoveAsync`, `HardDeleteAsync`. |
| `IBaseRepository<TEntity>` | Composition of the read + write repositories. |
| `IDapperRepository<TEntity>` | Raw/high-performance access: paged reads, `FindAsync`, raw SQL, stored procedures (incl. output params). |
| `IUnitOfWork` | Transaction boundary: `SaveChangesAsync`, `Begin/Commit/RollbackTransaction`, `ExecuteTransactionAsync(...)`, `Detach`. |
| `IAuthService` | Current-user/role identity resolved from JWT claims (`GetUserId`, `GetRoleId`, `GetRolesGuid`, `CheckRoleValidation`, …). |
| `IErrorLogger` | `LogError(Exception, object?)` plus error-file path resolution. |

All repository contracts are constrained to `where TEntity : BaseEntity`.

### Base models (`BaseModels`)

| Type | Description |
| --- | --- |
| `BaseEntity` | Root entity with an identity plus soft-delete/audit fields (`IsDelete`, `CreateDate`, `CreateUserId`, `UpdateDate`, …). Mutations go through explicit `SetCreate` / `SetUpdate` / `SetDelete` methods — setters are private. |
| `AggregateRoot` | `BaseEntity` that collects `BaseDomainEvent`s (`AddDomainEvent` / `RemoveDomainEvent`) for later dispatch. |
| `BaseDomainEvent` | MediatR `INotification` carrying audit metadata. |
| `BaseDto` | Marker base type for DTOs (used by the grid query abstractions). |
| `OperationResult` / `OperationResult<T>` | Standardized command result with a factory API (`Success`, `Error`, `NotFound`, `NoContent`, `UnAuthorize`, `Forbidden`) and an `OperationResultStatus` code. |
| `OperationQueryResult` / `OperationQueryResult<T>` | Query-side equivalents of the above. |
| `SystemError` | Persisted error record (user, roles, message, file, …). |
| `MongoDbConfig` | Connection-string / database-name options for MongoDB logging. |
| `ApiResultHttp` / `ApiResultHttp<T>` | HTTP envelope consumed by `Despro.Framework.WebClient`. |

### Grid, filtering & paging (`BaseModels/GridData`)

- `BaseGrid` — pagination + ordering + `FilterParam` list; can hydrate itself from a JSON
  string (`Set(string)`) with sensible defaults.
- `GridData<TData>` — a materialized page (`Data`, `EntityCount`, `PageCount`).
- `FilterService` — `IQueryable<T>` extensions (`FilterList`, `PagingList`,
  `FilterPagingList`) that translate `BaseGrid` filters into EF Core expression trees,
  including nested property paths, collection `Any(...)`, case-insensitive `LIKE`, enum,
  numeric, decimal, bool and date comparisons.

```csharp
var page = dbSet.AsQueryable()
                .FilterPagingList(baseGrid)   // apply filters + ordering + skip/take
                .ProjectToType<ProductDto>();
```

### Exceptions (`BaseExceptions`)

An abstract exception hierarchy that the higher layers extend and the presentation
middleware maps to HTTP status codes:
`BaseException`, `BaseForbiddenException`, `BaseInvalidDataException`, `BaseNotFoundException`.

### Extensions & helpers (`BaseExtensions`)

- `IncludeExtensions` — `IncludeFiltered` / `ThenIncludeFiltered` that transparently filter
  out soft-deleted (`IsDelete`) navigations.
- `PersianConvertorDate` — Gregorian ⇄ Shamsi (Jalali) conversion (`ToShamsi`,
  `ToShamsiWithTime`, `ShamsiToMiladi`).
- `StringExtensions` — Persian/Arabic digit & character normalization.
- `DateTimeExtension` (`IsBetween`), `MoneyHelper` (Tooman formatting).

### Validators (`Validator`)

- `FileValidator` — a configurable `AbstractValidator<IFormFile>` (max size, allowed
  extensions, required/optional) with localized failure messages.

### Constants

`Constants.HttpClientName` (named `HttpClient`) and `Constants.RoleIdKey`.

---

## Usage

```csharp
public class Product : AggregateRoot          // gets Id, audit fields, domain events
{
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
}

public class ProductDto : BaseDto             // eligible for grid queries
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// Command handlers return standardized results:
return OperationResult<ProductDto>.Success(dto);
return OperationResult.NotFound();
```

---

## Project structure

```
Despro.Framework.Base
├── BaseExceptions/      # abstract exception hierarchy
├── BaseExtensions/      # IQueryable includes, Persian date, string/date/money helpers
├── BaseModels/          # BaseEntity, AggregateRoot, OperationResult, DTOs
│   ├── DbModels/        # MongoDbConfig
│   ├── GridData/        # BaseGrid, GridData<>, FilterService
│   └── HttpModels/      # ApiResultHttp envelope
├── IBaseServices/       # repository / UoW / auth / logger contracts
│   └── IDbServices/     # IDapperRepository
├── Validator/           # FileValidator
└── Constants.cs
```
