# Despro.Framework.WebClient

> The typed **HTTP client** layer of [Despro Framework](../README.md).

`Despro.Framework.WebClient` provides a typed outbound HTTP client (`IHttp` / `Http`) for
service-to-service integration. It serializes with Newtonsoft.Json, forwards the current
user's bearer token (via `IAuthService`), unwraps the framework's `ApiResultHttp` envelope,
and is registered over a named `HttpClient`.

- **Package:** `Despro.Framework.WebClient`
- **Version:** `2.0.4`
- **Target framework:** `net10.0`
- **Depends on:** `Despro.Framework.Base`

---

## Installation

```bash
dotnet add package Despro.Framework.WebClient
```

### DI registration

```csharp
using Despro.Framework.WebClient;

builder.Services.AddFrameworkWebClient();
```

`AddFrameworkWebClient` registers `IHttp → Http` (scoped) and configures a named
`HttpClient` (`Constants.HttpClientName`) with an `application/json` Accept header and a
5-minute timeout.

> **Note:** the configured primary handler disables server-certificate validation
> (`ServerCertificateCustomValidationCallback => true`), which is intended for trusted
> internal integrations. Review this before using it against untrusted endpoints.

---

## `IHttp`

A typed client covering the common HTTP verbs plus form and file operations. Every method
accepts `apiResult` (unwrap the `ApiResultHttp` envelope), `isAuth` (attach the bearer
token), an explicit `token`, and `isRole` flags.

| Method | Purpose |
| --- | --- |
| `GetAsync<TOut>` | GET and deserialize `TOut`. |
| `PostAsync<TIn, TOut>` / `PutAsync` / `PatchAsync` | JSON body request/response. |
| `PostFormAsync<TIn, TOut>` / `PutFormAsync` / `PatchFormAsync` | `multipart/form-data` from a model's properties. |
| `DeleteAsync<TOut>` | DELETE and deserialize `TOut`. |
| `GetFileAsync` | Download a response as a `MemoryStream`. |
| `UploadFile<TOut>` | Upload an `IFormFile`. |

On non-success responses the client throws `WebClientException`.

---

## Usage

```csharp
public class CatalogClient(IHttp http)
{
    public Task<ProductDto> GetProductAsync(long id)
        => http.GetAsync<ProductDto>($"https://catalog.internal/api/v1/products/{id}");

    public Task<long> CreateProductAsync(CreateProductRequest request)
        => http.PostAsync<CreateProductRequest, long>("https://catalog.internal/api/v1/products", request);
}
```

By default (`isAuth: true`) the current request's bearer token is forwarded to the
downstream service; pass an explicit `token` to override it.

---

## Exceptions

- `WebClientBaseException` (abstract, extends `Base`'s `BaseException`).
- `WebClientException` — thrown on HTTP failures / non-success responses.

---

## Project structure

```
Despro.Framework.WebClient
├── IRepository/            # IHttp contract
├── Repository/             # Http implementation
├── WebClientExceptions/    # WebClientBaseException, WebClientException
└── FrameworkWebClientDi.cs # AddFrameworkWebClient
```
