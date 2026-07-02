# Despro.Framework.Presentation.Api

> The **controller-based** presentation flavor of [Despro Framework](../README.md).

`Despro.Framework.Presentation.Api` specializes
[`Despro.Framework.Presentation`](../Despro.Framework.Presentation/README.md) for
**controller-based** Web APIs. It provides a base `ApiController` that turns domain
`OperationResult`/`OperationQueryResult` values into the standardized `ApiResult` envelope,
plus a route-prefix convention and Newtonsoft-based JSON configuration.

- **Package:** `Despro.Framework.Presentation.Api`
- **Version:** `2.0.8`
- **Target framework:** `net10.0`
- **Depends on:** `Despro.Framework.Presentation`

---

## Installation

```bash
dotnet add package Despro.Framework.Presentation.Api
```

### Setup

```csharp
using Despro.Framework.Presentation;
using Despro.Framework.Presentation.Api;

// shared presentation services (JWT, Swagger, CORS, Mapster, HSTS)
builder.Services.AddFrameworkPresentationWeb(builder.Configuration, "My Application", "DefaultCorsPolicy");
// controller-based services
builder.Services.AddFrameworkPresentationWebApi(RoutePrefix: "v{version:apiVersion}/[controller]");

var app = builder.Build();
app.UseFrameworkPresentationWeb(ShowSwaggerInProduction: false);
app.UseFrameworkPresentationWebApi(); // maps controllers
app.Run();
```

`AddFrameworkPresentationWebApi` configures controllers with a `RoutePrefixConvention`,
Newtonsoft JSON (`DefaultContractResolver`, null naming policy) and converts invalid
`ModelState` into a thrown exception (handled by the shared exception middleware).
`UseFrameworkPresentationWebApi` calls `MapControllers()`.

---

## Key components

### `ApiController` (`ControllerTools`)

An `[ApiController]` / `[ApiVersion(1.0)]` base `ControllerBase` with helpers that build an
`ApiResult`, set the HTTP status code from the mapped `AppStatusCode`, and cover the command
and query cases:

| Method | Input → Output |
| --- | --- |
| `CommandResult(OperationResult)` | command with no payload |
| `CommandResult<TData>(OperationResult<TData>)` | command with payload |
| `QueryResult<TData>(TData)` | raw query result (always success) |
| `QueryResult<TData>(OperationQueryResult<TData>)` | query with status |

### `RoutePrefixConvention` (`Utilites`)

An `IApplicationModelConvention` that applies the configured route prefix
(e.g. `v{version:apiVersion}/[controller]`) to every `[ApiController]`.

---

## Usage

```csharp
public class ProductsController(ISender sender) : ApiController
{
    [HttpPost]
    public async Task<ApiResult<long?>> Create(CreateProductCommand command)
        => CommandResult(await sender.Send(command));

    [HttpGet]
    public async Task<ApiResult<GridData<ProductDto>>> List([FromQuery] string grid)
    {
        var baseGrid = new BaseGrid();
        baseGrid.Set(grid); // hydrate paging/filtering from the JSON query string
        return QueryResult(await sender.Send(new ProductGridQuery(baseGrid)));
    }
}
```

Every response is a consistent `ApiResult` with the correct HTTP status code.

---

## Project structure

```
Despro.Framework.Presentation.Api
├── ControllerTools/            # ApiController base
├── Utilites/                   # RoutePrefixConvention
├── FrameworkPresentationWebDi.cs     # AddFrameworkPresentationWebApi
└── FrameworkPresentationWebUseApp.cs # UseFrameworkPresentationWebApi
```
