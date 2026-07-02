# Despro Framework

**Despro Framework** is a multi-layered enterprise framework built on **.NET 10.0** and
the principles of **Clean Architecture**. It packages cross-cutting infrastructure —
data access, CQRS, authentication, auditing and typed HTTP clients — into a set of
reusable, independently versioned **NuGet packages** so that product teams can bootstrap
an enterprise-grade Web API (controller-based or Minimal API) with a few
dependency-injection calls.

- **Target framework:** `net10.0`
- **Core package version (`Despro.Framework.Base`):** `2.1.0`
- **Distribution:** NuGet feed

---

## Features

- **Data access** — generic repository abstractions over **EF Core**
  (`IBaseRepository<>` / `Repository<>`, `IUnitOfWork`) and **Dapper**
  (`IDapperRepository<>`), exposed through a unified `IRepositoryServices` entry point.
- **CQRS** — command/query handling via **MediatR**, with a custom dispatcher
  (`ICustomPublisher` / `CustomPublisher`) and a validation pipeline behavior
  (`CommandValidationBehavior<,>`) backed by FluentValidation.
- **Authentication & authorization** — JWT bearer authentication and role-based access
  control through `IAuthService` / `AuthService`, resolving identity and roles from JWT
  claims.
- **Password policy** — a configurable, self-validating password policy
  (`AuthPasswordOptions`) bound from configuration and validated on start-up.
- **Audit logging** — deferred bulk logging persisted to **MongoDB** (`ILogService` /
  `MongoLogService`, `ILoggingContext`), with no-op implementations
  (`NullLogService` / `NullLoggingContext`) when logging is disabled.
- **Typed HTTP client** — `Despro.Framework.WebClient` exposes a typed `IHttp` / `Http`
  client registered over a named `HttpClient` for outbound HTTP integration.

---

## Architecture

Despro follows a Clean Architecture layering where dependencies flow inward toward the
shared core abstractions:

```
Base → Domain → Application → Infrastructure → Presentation
```

`Base` holds the cross-cutting abstractions (CQRS interfaces, base models, validators,
shared utilities) that every other layer depends on. Concrete implementations live in
`Infrastructure`, while `Presentation` (and its `Api` / `MinimalApi` specializations)
exposes the HTTP surface.

The framework is intentionally opinionated about how requests flow through an application:

- **Repository / Unit of Work** — `IBaseRepository<>` / `Repository<>` provide generic
  EF Core persistence, coordinated through `IUnitOfWork` so a logical operation commits
  as a single transaction. `IRepositoryServices` groups the repository entry points, and
  `IDapperRepository<>` is available for raw, high-performance reads.
- **MediatR pipeline** — commands and queries are dispatched through MediatR. The
  `CommandValidationBehavior<,>` pipeline behavior runs FluentValidation validators before
  a handler executes, short-circuiting invalid commands. `CustomPublisher`
  (`ICustomPublisher`) provides a tailored notification dispatch strategy on top of MediatR.
- **JWT authentication** — `AuthService` (`IAuthService`) resolves the current user and
  roles from JWT claims, giving handlers a consistent identity abstraction independent of
  the transport.
- **MongoDB audit logging** — log entries are buffered through `ILoggingContext` and
  flushed in bulk by `MongoLogService`. When logging is disabled the no-op
  `NullLogService` / `NullLoggingContext` implementations are wired up instead, so calling
  code never has to branch on whether logging is enabled.
- **Password policy** — `AuthPasswordOptions` centralizes password rules (length,
  distinct characters, character-class requirements) and is validated on start-up so a
  misconfiguration fails fast rather than at first use.

### Projects

| Project | Version | Role |
| --- | --- | --- |
| `Despro.Framework.Base` | 2.1.0 | Core abstractions: CQRS interfaces, base models, validators, shared utilities. |
| `Despro.Framework.Domain` | 2.0.4 | Domain models, value objects and the password-policy options. |
| `Despro.Framework.Application` | 2.0.5 | Application layer: MediatR pipeline behaviors and command/query tooling. |
| `Despro.Framework.Infrastructure` | 2.1.3 | Data persistence (EF Core, Dapper), security, MongoDB logging, DI wiring. |
| `Despro.Framework.Presentation` | 2.10.1 | JWT auth, CORS, Swagger, Mapster and API versioning setup. |
| `Despro.Framework.Presentation.Api` | 2.0.8 | Controller-based presentation specialization. |
| `Despro.Framework.Presentation.MinimalApi` | 2.10.0 | Minimal API endpoint registration and routing. |
| `Despro.Framework.WebClient` | 2.0.4 | Typed HTTP client integration. |

### Project relationships

The actual `ProjectReference` graph between the projects:

```mermaid
graph TD
    Base[Despro.Framework.Base]
    Domain[Despro.Framework.Domain]
    Application[Despro.Framework.Application]
    Infrastructure[Despro.Framework.Infrastructure]
    Presentation[Despro.Framework.Presentation]
    PresentationApi[Despro.Framework.Presentation.Api]
    PresentationMinimalApi[Despro.Framework.Presentation.MinimalApi]
    WebClient[Despro.Framework.WebClient]

    Domain --> Base
    Application --> Base
    Infrastructure --> Base
    Presentation --> Base
    WebClient --> Base
    PresentationApi --> Presentation
    PresentationMinimalApi --> Presentation
```

---

## Installation / NuGet

All packages are published to the **`Nuget`** feed:

```
https://api.nuget.org/v3/index.json
```

### Install packages

```bash
dotnet add package Despro.Framework.Base
dotnet add package Despro.Framework.Application
dotnet add package Despro.Framework.Infrastructure
dotnet add package Despro.Framework.Presentation

# Pick the presentation flavor your project uses:
dotnet add package Despro.Framework.Presentation.Api          # controller-based
dotnet add package Despro.Framework.Presentation.MinimalApi   # minimal API

# Optional add-ons:
dotnet add package Despro.Framework.Domain
dotnet add package Despro.Framework.WebClient
```

---

## Getting Started / DI Bootstrap

Register the framework services in `Program.cs` using the framework's extension methods.
The infrastructure registration needs the assemblies that contain your use cases
(commands) and queries so MediatR handlers and validators can be discovered.

```csharp
using System.Reflection;
using Despro.Framework.Application;
using Despro.Framework.Domain;
using Despro.Framework.Infrastructure;
using Despro.Framework.Presentation;
using Despro.Framework.Presentation.Api;          // or Despro.Framework.Presentation.MinimalApi
using Despro.Framework.WebClient;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

// Assemblies that contain your MediatR use cases (commands) and queries.
var useCaseAssembly = Assembly.GetExecutingAssembly();
var queryAssembly = Assembly.GetExecutingAssembly();

// Infrastructure: repositories, auth, error logging, MediatR, MongoDB audit logging.
services.AddFrameworkInfrastructure(
    configuration,
    useCaseAssembly,
    queryAssembly,
    MongoDbLog: true); // set false to disable MongoDB logging (uses no-op log services)

// Application: MediatR validation pipeline behavior + FluentValidation validators.
services.AddFrameworkApplication();

// Domain: bind and validate the password policy (AuthPasswordOptions) on start-up.
services.AddFrameworkDomain(configuration);

// Presentation: JWT authentication, Swagger, Mapster, CORS, HSTS, memory cache.
services.AddFrameworkPresentationWeb(
    configuration,
    ApplicationName: "My Application",
    CorsPolicyName: "DefaultCorsPolicy");

// Controller-based presentation:
services.AddFrameworkPresentationWebApi(RoutePrefix: "v{version:apiVersion}/[controller]");

// --- OR --- Minimal API presentation (registers BaseEndpoint implementations):
// services.AddFrameworkPresentationWebMinimalApi(
//     ApiAssembly: Assembly.GetExecutingAssembly(),
//     RoutePrefix: "v{version:apiVersion}/[controller]");

// Optional: typed HTTP client (IHttp / Http) over a named HttpClient.
services.AddFrameworkWebClient();

var app = builder.Build();
app.Run();
```

### Extension method signatures

| Method | Source | Signature |
| --- | --- | --- |
| `AddFrameworkInfrastructure` | `Despro.Framework.Infrastructure/FrameworkInfrastructureDi.cs` | `(this IServiceCollection services, IConfiguration configuration, Assembly useCaseAssembly, Assembly queryAssembly, bool MongoDbLog = false)` |
| `AddFrameworkApplication` | `Despro.Framework.Application/FrameworkApplicationDi.cs` | `(this IServiceCollection services)` |
| `AddFrameworkDomain` | `Despro.Framework.Domain/FrameworkDomainDi.cs` | `(this IServiceCollection services, IConfiguration configuration)` |
| `AddFrameworkPresentationWeb` | `Despro.Framework.Presentation/FrameworkPresentationWebDi.cs` | `(this IServiceCollection services, IConfiguration configuration, string ApplicationName, string CorsPolicyName)` |
| `AddFrameworkPresentationWebApi` | `Despro.Framework.Presentation.Api/FrameworkPresentationWebDi.cs` | `(this IServiceCollection services, string RoutePrefix)` |
| `AddFrameworkPresentationWebMinimalApi` | `Despro.Framework.Presentation.MinimalApi/FrameworkPresentationWebDi.cs` | `(this IServiceCollection services, Assembly ApiAssembly, string RoutePrefix)` |
| `AddFrameworkWebClient` | `Despro.Framework.WebClient/FrameworkWebClientDi.cs` | `(this IServiceCollection services)` |

`AddFrameworkDomain` binds the `AuthPasswordOptions` section, validates its data
annotations, and additionally enforces that `RequiredUniqueChars` never exceeds
`RequiredLength`. Because validation runs on start-up (`ValidateOnStart`), an invalid
password policy causes the application to fail fast at boot rather than at first use.

`AddFrameworkWebClient` registers the typed `IHttp` / `Http` client and configures a named
`HttpClient` (JSON `Accept` header, 5-minute timeout) for outbound HTTP calls.

---

## Configuration

The framework reads the following keys from `appsettings.json` (or any configured
`IConfiguration` source).

| Section / Key | Used by | Notes |
| --- | --- | --- |
| `MongoDbConfig:ConnectionString` | `AddFrameworkInfrastructure` (when `MongoDbLog: true`) | Required when MongoDB logging is enabled; throws if missing. |
| `MongoDbConfig:DatabaseName` | `AddFrameworkInfrastructure` (when `MongoDbLog: true`) | Database used for audit logs. |
| `AuthPasswordOptions:RequiredLength` | `AddFrameworkDomain` | Minimum password length (4–128, default 6). |
| `AuthPasswordOptions:RequiredUniqueChars` | `AddFrameworkDomain` | Minimum distinct characters (≥ 1, default 1). Must not exceed `RequiredLength`. |
| `JwtConfig:SignInKey` | `AddJwtAuthentication` (`Despro.Framework.Presentation/Utilites`) | Symmetric signing key (UTF-8). |
| `JwtConfig:Issuer` | `AddJwtAuthentication` | Validated token issuer. |
| `JwtConfig:Audience` | `AddJwtAuthentication` | Validated token audience. |
| `App:CorsOrigins` | `AddFrameworkPresentationWeb` | Comma-separated list of allowed origins. |

### Example `appsettings.json`

```json
{
  "MongoDbConfig": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "DesproLogs"
  },
  "AuthPasswordOptions": {
    "RequiredLength": 8,
    "RequiredUniqueChars": 4,
    "RequireNonAlphanumeric": true,
    "RequireLowercase": true,
    "RequireUppercase": true,
    "RequireDigit": true
  },
  "JwtConfig": {
    "SignInKey": "replace-with-a-long-random-secret-key",
    "Issuer": "https://your-app.example.com",
    "Audience": "https://your-app.example.com"
  },
  "App": {
    "CorsOrigins": "https://localhost:5173,https://app.example.com"
  }
}
```

> JWT validation enables issuer, audience, lifetime and signing-key checks with zero clock
> skew. A bearer token may also be supplied via an `access_token` query string parameter.

---

## Tech Stack / Dependencies

Versions are taken from the project files.

| Package | Version | Used in |
| --- | --- | --- |
| MediatR | 14.0.0 | Base, Application, Infrastructure |
| FluentValidation | 12.1.1 | Base, Infrastructure |
| FluentValidation.DependencyInjectionExtensions | 12.1.1 | Base, Infrastructure |
| Mapster | 7.4.0 | Base |
| Microsoft.EntityFrameworkCore | 10.0.2 | Base, Infrastructure |
| Microsoft.EntityFrameworkCore.SqlServer | 10.0.2 | Infrastructure |
| Dapper | 2.1.66 | Infrastructure |
| MongoDB.Driver | 3.6.0 | Infrastructure |
| Newtonsoft.Json | 13.0.4 | Base |
| Microsoft.AspNetCore.Mvc.NewtonsoftJson | 10.0.2 | Presentation |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.2 | Presentation |
| System.IdentityModel.Tokens.Jwt | 8.15.0 | Infrastructure, Presentation |
| Microsoft.AspNetCore.OpenApi | 10.0.2 | Presentation |
| Swashbuckle.AspNetCore (+ Swagger / SwaggerGen) | 10.1.0 | Presentation |
| Asp.Versioning.Http | 8.1.1 | Presentation |
| Asp.Versioning.Mvc.ApiExplorer | 8.1.1 | Presentation |
| Microsoft.Extensions.DependencyInjection.Abstractions | 10.0.2 | WebClient |

---

## Project Structure

Solution: [`DesproFramework.sln`](DesproFramework.sln)

```
Despro.Framework.Base
Despro.Framework.Domain
Despro.Framework.Application
Despro.Framework.Infrastructure
Despro.Framework.Presentation
Despro.Framework.Presentation.Api
Despro.Framework.Presentation.MinimalApi
Despro.Framework.WebClient
```
