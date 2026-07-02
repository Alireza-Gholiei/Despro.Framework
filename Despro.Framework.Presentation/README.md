# Despro.Framework.Presentation

> The shared **presentation** layer of [Despro Framework](../README.md).

`Despro.Framework.Presentation` contains the HTTP-facing cross-cutting concerns shared by
both presentation flavors (`Presentation.Api` for controllers and `Presentation.MinimalApi`
for minimal endpoints): JWT authentication, Swagger/OpenAPI with API versioning, CORS, HSTS,
Mapster configuration, Persian localization, the standardized `ApiResult` envelope and a
global exception-handling middleware.

- **Package:** `Despro.Framework.Presentation`
- **Version:** `2.10.1`
- **Target framework:** `net10.0`
- **Depends on:** `Despro.Framework.Base`

---

## Installation

```bash
dotnet add package Despro.Framework.Presentation
```

Normally you install one of the concrete flavors
([`Presentation.Api`](../Despro.Framework.Presentation.Api/README.md) or
[`Presentation.MinimalApi`](../Despro.Framework.Presentation.MinimalApi/README.md)) which
reference this package transitively.

### Setup

```csharp
using Despro.Framework.Presentation;

// ── Service registration ──
builder.Services.AddFrameworkPresentationWeb(
    builder.Configuration,
    ApplicationName: "My Application",
    CorsPolicyName: "DefaultCorsPolicy");

// ── Middleware pipeline ──
var app = builder.Build();
app.UseFrameworkPresentationWeb(ShowSwaggerInProduction: false);
```

`AddFrameworkPresentationWeb` wires up JWT authentication, Swagger, Mapster, a named CORS
policy (origins from `App:CorsOrigins`), in-memory caching and HSTS.
`UseFrameworkPresentationWeb` initializes Persian culture, forwarded headers, Swagger UI
(dev, or prod when enabled), HTTPS redirection, routing, CORS, auth, static files and the
API exception handler.

---

## Key components

### Response envelope (`ControllerTools`)

- `ApiResult` / `ApiResult<TData>` with `MetaData` (`Message`, `AppStatusCode`) — the
  uniform response shape returned to clients.
- `AppStatusCode` — application status enum (`Success`, `NoContent`, `BadRequest`,
  `UnAuthorize`, `Forbidden`, `NotFound`, `InvalidData`, `ServerError`).
- `ModelStateUtil` / `ModelStateUtilites` — flatten `ModelState` errors into a message.

### Status mapping (`PresentationApiExtensions`)

`HttpEnumHelper` maps `OperationResultStatus` → `AppStatusCode` (`MapOperationStatus`) and
`AppStatusCode` → typed HTTP results (`GetHttpStatusCode`), used by both presentation
flavors to translate domain results into HTTP responses.

### Global exception handling (`Middlewares`)

`ApiExceptionHandlerMiddleware` (registered via `UseApiExceptionHandler`) catches the
framework's exception hierarchy (`BaseException`, `BaseForbiddenException`,
`BaseInvalidDataException`, `BaseNotFoundException`), maps each to the right
`AppStatusCode`, logs via `IErrorLogger`, and writes a JSON `ApiResult`.

### Utilities (`Utilites`)

| Helper | Purpose |
| --- | --- |
| `AddJwtAuthentication` | JWT bearer auth from `JwtConfig:*`; validates issuer/audience/lifetime/signing key with zero clock skew; also accepts a token via the `access_token` query string. |
| `AddSwagger` | API versioning (`UrlSegmentApiVersionReader`, `'v'VVV` format) + Swagger with a Bearer security scheme. |
| `ConfigureSwaggerOptions` | Builds a Swagger doc per discovered API version. |
| `AddMapsterConfig` | Registers `IMapper` and global `DateTime ⇄ long (Ticks)` mappings. |
| `DatePersian` | Initializes the `fa-IR` Persian (Jalali) culture. |

---

## Configuration

| Key | Used by | Notes |
| --- | --- | --- |
| `JwtConfig:SignInKey` | `AddJwtAuthentication` | Symmetric signing key (UTF-8). |
| `JwtConfig:Issuer` | `AddJwtAuthentication` | Validated issuer. |
| `JwtConfig:Audience` | `AddJwtAuthentication` | Validated audience. |
| `App:CorsOrigins` | `AddFrameworkPresentationWeb` | Comma-separated allowed origins (throws if empty). |

---

## Project structure

```
Despro.Framework.Presentation
├── ControllerTools/            # ApiResult, AppStatusCode, ModelState helpers
├── Middlewares/                # ApiExceptionHandlerMiddleware
├── PresentationApiExtensions/  # HttpEnumHelper (status mapping)
├── PresentationExceptions/     # BasePresentationException
├── Utilites/                   # JWT, Swagger, Mapster, Persian culture
├── FrameworkPresentationWebDi.cs     # AddFrameworkPresentationWeb
└── FrameworkPresentationWebUseApp.cs # UseFrameworkPresentationWeb
```
