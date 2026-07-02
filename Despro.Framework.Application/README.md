# Despro.Framework.Application

> The **application** (CQRS) layer of [Despro Framework](../README.md).

`Despro.Framework.Application` defines the **CQRS contracts** (commands, queries and their
handlers) built on MediatR, plus the FluentValidation pipeline behavior that validates every
command before it reaches a handler. This is the layer your use cases build directly on.

- **Package:** `Despro.Framework.Application`
- **Version:** `2.0.5`
- **Target framework:** `net10.0`
- **Depends on:** `Despro.Framework.Base`, MediatR 14.0.0

---

## Installation

```bash
dotnet add package Despro.Framework.Application
```

### DI registration

```csharp
using Despro.Framework.Application;

builder.Services.AddFrameworkApplication();
```

`AddFrameworkApplication` registers the `CommandValidationBehavior<,>` MediatR pipeline
behavior and scans this assembly for FluentValidation validators. (The assemblies that
contain *your* handlers/validators are registered separately by
`AddFrameworkInfrastructure`.)

---

## CQRS contracts (`QueryCommandTools`)

### Commands — state-changing operations

| Contract | Returns |
| --- | --- |
| `ICommand` | `OperationResult` |
| `ICommand<TResponse>` | `OperationResult<TResponse>` |
| `ICommandHandler<TCommand>` | handles `ICommand` |
| `ICommandHandler<TCommand, TResponseData>` | handles `ICommand<TResponseData>` |

### Queries — read operations

| Contract | Returns |
| --- | --- |
| `IQuery<TResponse>` | `TResponse` |
| `IQueryOperation<TResponse>` | `OperationQueryResult<TResponse>` |
| `QueryGrid<TResponse>` (abstract, `TResponse : BaseDto`) | `GridData<TResponse>` |
| `QueryGridOperation<TResponse>` (abstract, `TResponse : BaseDto`) | `OperationQueryResult<GridData<TResponse>>` |

Each query contract has a matching handler interface: `IQueryHandler<,>`,
`IQueryOperationHandler<,>`, `IQueryGridHandler<,>`, `IQueryGridOperationHandler<,>`.

### Validation pipeline

`CommandValidationBehavior<TRequest, TResponse>` runs all registered
`IValidator<TRequest>` instances before the handler executes. If any rule fails it
aggregates the messages and throws `InvalidCommandException`, short-circuiting the pipeline
so handlers only ever run against valid input.

### Exceptions (`ApplicationExceptions`)

- `BaseApplicationException` (abstract, extends `Base`'s `BaseException`).
- `InvalidCommandException` — thrown by the validation behavior on validation failure.

---

## Usage

```csharp
// Command
public record CreateProductCommand(string Name, decimal Price) : ICommand<long>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
    }
}

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, long>
{
    public async Task<OperationResult<long>> Handle(CreateProductCommand request, CancellationToken ct)
    {
        // ... persist and return
        return OperationResult<long>.Success(newId);
    }
}

// Grid query
public class ProductGridQuery(BaseGrid grid) : QueryGrid<ProductDto>(grid);

public class ProductGridQueryHandler : IQueryGridHandler<ProductGridQuery, ProductDto>
{
    public Task<GridData<ProductDto>> Handle(ProductGridQuery request, CancellationToken ct) { /* ... */ }
}
```

Dispatch through MediatR's `ISender`/`IMediator`; the validation behavior runs automatically.

---

## Project structure

```
Despro.Framework.Application
├── ApplicationExceptions/   # BaseApplicationException, InvalidCommandException
├── QueryCommandTools/       # ICommand, IQuery, QueryGrid, CommandValidationBehavior
└── FrameworkApplicationDi.cs # AddFrameworkApplication
```
