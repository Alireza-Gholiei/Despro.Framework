# Despro.Framework.Presentation.MinimalApi

> The **Minimal API** presentation flavor of [Despro Framework](../README.md).

`Despro.Framework.Presentation.MinimalApi` specializes
[`Despro.Framework.Presentation`](../Despro.Framework.Presentation/README.md) for **Minimal
API** endpoints. It provides a class-based endpoint model (`BaseEndpoint` / `IEndpoint`) with
automatic discovery, API-version-aware route grouping, and helpers that translate domain
results into typed HTTP results wrapped in the standardized `ApiResult` envelope.

- **Package:** `Despro.Framework.Presentation.MinimalApi`
- **Version:** `2.10.0`
- **Target framework:** `net10.0`
- **Depends on:** `Despro.Framework.Presentation`

---

## Installation

```bash
dotnet add package Despro.Framework.Presentation.MinimalApi
```

### Setup

```csharp
using System.Reflection;
using Asp.Versioning;
using Despro.Framework.Presentation;
using Despro.Framework.Presentation.MinimalApi;

// shared presentation services (JWT, Swagger, CORS, Mapster, HSTS)
builder.Services.AddFrameworkPresentationWeb(builder.Configuration, "My Application", "DefaultCorsPolicy");
// minimal-API services: discovers IEndpoint implementations in the given assembly
builder.Services.AddFrameworkPresentationWebMinimalApi(
    ApiAssembly: Assembly.GetExecutingAssembly(),
    RoutePrefix: "v{version:apiVersion}/[controller]");

var app = builder.Build();
app.UseFrameworkPresentationWeb(ShowSwaggerInProduction: false);
app.UseFrameworkPresentationWebMinimalApi(new[] { new ApiVersion(1, 0) }); // maps endpoints
app.Run();
```

`AddFrameworkPresentationWebMinimalApi` scans `ApiAssembly` for `IEndpoint` implementations
(registered scoped) and sets JSON options to preserve property names.
`UseFrameworkPresentationWebMinimalApi` builds an API version set, resolves every registered
endpoint and calls `MapEndpoint` for each.

---

## Key components

### `IEndpoint` / `BaseEndpoint` (`ControllerTools`)

`BaseEndpoint` is the abstract base for a group of endpoints. Override `DefineEndpoints` to
map routes; the base class handles version-set binding, route grouping, tagging and OpenAPI
metadata. Optional overrides:

| Member | Default | Purpose |
| --- | --- | --- |
| `Route` | `null` | Explicit group path; otherwise derived from the route prefix + tag. |
| `Tag` | `null` (class name minus `Endpoints`) | Swagger tag / route segment. The class name **must** end with `Endpoints` when no `Tag` is set. |
| `GroupName` | `null` | Optional group name. |
| `Version` | `1.0` | API version for the group. |

Result helpers return strongly-typed `Results<Ok<ApiResult<T>>, NoContent, BadRequest<…>,
UnauthorizedHttpResult, ForbidHttpResult, NotFound<…>>`:

- `CommandResult(OperationResult)` / `CommandResult<TData>(OperationResult<TData>)`
- `QueryResult<TData>(TData)` / `QueryResult<TData>(OperationQueryResult<TData>)`

### Endpoint discovery (`Utilites`)

`EndpointExtensions.AddAllEndpoints(assembly)` registers every non-abstract `IEndpoint`
implementation in the assembly as a scoped service.

---

## Usage

```csharp
public class ProductsEndpoints : BaseEndpoint   // class name must end with "Endpoints"
{
    protected override void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/", async (CreateProductCommand command, ISender sender)
            => CommandResult(await sender.Send(command)));

        app.MapGet("/{id:long}", async (long id, ISender sender)
            => QueryResult(await sender.Send(new GetProductQuery(id))));
    }
}
```

The endpoint is discovered automatically and mapped under
`v{version}/Products` with the Swagger tag `Products`.

### Exceptions

`PresentationException` (internal, extends the shared `BasePresentationException`) is thrown
when an endpoint class is misnamed (does not end with `Endpoints`).

---

## Project structure

```
Despro.Framework.Presentation.MinimalApi
├── ControllerTools/            # IEndpoint, BaseEndpoint
├── PresentationExceptions/     # PresentationException
├── Utilites/                   # AddAllEndpoints (discovery)
├── FrameworkPresentationWebDi.cs     # AddFrameworkPresentationWebMinimalApi
└── FrameworkPresentationWebUseApp.cs # UseFrameworkPresentationWebMinimalApi
```
