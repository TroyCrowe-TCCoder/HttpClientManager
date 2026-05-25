# HttpClientManager

A .NET 10 class library that provides a clean, DI-friendly abstraction over `IHttpClientFactory` for consuming HTTP APIs. Designed to be consumed by front-end or orchestration applications, it centralises all HTTP concerns — client creation, request building, and response processing — behind well-defined interfaces.

## Installation

```shell
dotnet add package HttpClientManager
```

Register the library in your host setup:

```csharp
builder.Services.AddHttpClientManager();
```

---

## Local Validation

Run the repository-local validation entry point before creating or updating a pull request:

pwsh ./scripts/validate.ps1 -Pack

Pipeline validation note: documentation-only changes should keep ChangeDetection false so Build and Test are skipped while PR completion can still proceed after approval.

This repository follows the class-library promotion model. The validation entry point restores, builds, optionally runs tests when test projects exist, and produces the traceable package output used for promotion.

---

## Table of Contents

- [Features](#features)
- [Requirements](#requirements)
- [Architecture Overview](#architecture-overview)
- [Getting Started](#getting-started)
  - [Dependency Injection Registration](#dependency-injection-registration)
  - [Manual Registration](#manual-registration)
- [Usage](#usage)
  - [Building Clients](#building-clients)
  - [Building Requests](#building-requests)
  - [Processing Requests](#processing-requests)
- [Class Reference](#class-reference)
- [Security Considerations](#security-considerations)
- [Contributing](#contributing)
- [Changelog](#changelog)

---

## Features

- **Centralised HTTP client creation** via `HttpClientBuilder` — basic, OAuth Bearer, and file-upload clients
- **Strongly typed request builders** — GET, POST (JSON + multipart), PUT, DELETE
- **Consistent response handling** — returns `(HttpStatusCode, string)` tuples for easy downstream deserialization
- **Fail-fast validation** — malformed relative URIs, unsafe file names, invalid OAuth base paths, and malformed Bearer tokens throw `ArgumentException` before dispatch
- **Bounded response reading** — body-reading methods enforce a 10 MB limit for both declared and chunked responses; streaming methods remain available for larger payloads
- **Library-friendly logging** — uses `ILogger<T>` for client-creation and request-dispatch diagnostics without logging tokens or other secrets
- **DI-native** — all components are interface-backed and registered with `IServiceCollection`
- **Async end-to-end** — all I/O operations are `Task`-based with no sync-over-async
- **Nullable enabled** — full C# nullable reference type support

---

## Requirements

| Requirement | Version |
|---|---|
| .NET | 9.0 |
| C# | 13.0 |
| `Microsoft.Extensions.Http` | 10.x |

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│                   Consuming Application                 │
└───────────────────────┬─────────────────────────────────┘
                        │ depends on interfaces
          ┌─────────────▼──────────────┐
          │     HttpClientManager      │
          │  (class library / NuGet)   │
          └──────┬──────────┬──────────┘
                 │          │
     ┌───────────▼──┐  ┌────▼────────────┐
     │ IHttpClient  │  │ IRequestManager │
     │   Builder    │  │  (build msgs)   │
     └───────┬──────┘  └────────┬────────┘
             │                  │
     ┌───────▼──────┐  ┌────────▼────────┐
     │HttpClientBld │  │ RequestManager  │
     │(creates HTTP │  │ (HttpRequest    │
     │   clients)   │  │  Message factory│
     └───────┬──────┘  └────────┬────────┘
             │                  │
             └────────┬─────────┘
                      │
              ┌───────▼────────┐
              │ IRequestProc.  │
              │(sends requests)│
              └───────┬────────┘
                      │
              ┌───────▼────────┐
              │RequestProcessor│
              │ (executes HTTP │
              │  operations)   │
              └───────┬────────┘
                      │
              ┌───────▼────────┐
              │  Downstream    │
              │  HTTP API      │
              └────────────────┘
```

Three focused responsibilities map to three interfaces:

| Interface | Implementation | Responsibility |
|---|---|---|
| `IHttpClientBuilder` | `HttpClientBuilder` | Creates configured `HttpClient` instances |
| `IRequestManager` | `RequestManager` | Builds `HttpRequestMessage` objects |
| `IRequestProcessor` | `RequestProcessor` | Executes HTTP calls and returns results |

---

## Getting Started

### Dependency Injection Registration

The library ships a static `HostBuilder.ConfigureServices()` helper that returns a pre-configured `IServiceCollection`:

```csharp
// In your application's DI setup (e.g., Program.cs)
var services = HttpClientManager.HostBuilder.ConfigureServices();

// Build and use the provider
var provider = services.BuildServiceProvider();
```

### Manual Registration

If you manage your own `IServiceCollection` (recommended for most host scenarios), register the components individually:

```csharp
builder.Services.AddHttpClient();
builder.Services.AddLogging();
builder.Services.AddScoped<Interfaces.IHttpClientBuilder, HttpClientBuilder>();
builder.Services.AddScoped<IRequestManager, RequestManager>();
builder.Services.AddScoped<IRequestProcessor, RequestProcessor>();
```

> **Note:** `IHttpClientFactory` is provided automatically by `AddHttpClient()`. `Interfaces.IHttpClientBuilder` is qualified to avoid ambiguity with `Microsoft.Extensions.DependencyInjection.IHttpClientBuilder`.
> The library also consumes `ILogger<T>` for minimal diagnostics, so manual registration should include `AddLogging()`.

---

## Usage

### Building Clients

Inject `IHttpClientBuilder` wherever you need an `HttpClient`:

```csharp
public class MyApiService
{
    private readonly IHttpClientBuilder _clientBuilder;

    public MyApiService(IHttpClientBuilder clientBuilder)
    {
        _clientBuilder = clientBuilder;
    }

    // Basic client (Accept: application/json)
    public HttpClient GetBasicClient()
        => _clientBuilder.CreateBasicClient("MyApi", "https://api.example.com/");

    // OAuth Bearer token client
    public HttpClient GetAuthClient(string token)
        => _clientBuilder.CreateOAuthClient("MyApi", "https://api.example.com/", token);

    // OAuth Bearer token client for file upload (no Accept header)
    public HttpClient GetFileClient(string token)
        => _clientBuilder.CreateOAuthClientWithFile("MyApi", "https://api.example.com/", token);
}
```

> The `clientType` string maps to a named HTTP client registration. See [named clients](https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory#named-clients) in `IHttpClientFactory` docs.

### Building Requests

Inject `IRequestManager` to construct `HttpRequestMessage` objects:

```csharp
// GET
var getRequest = _requestManager.GetRequest("v1/products/42");

// POST with JSON body
var postRequest = _requestManager.PostRequest("v1/products", jsonPayload);

// POST with multipart form data
var multipartRequest = _requestManager.PostRequest("v1/uploads", multipartContent);

// POST with a file attachment
var fileRequest = _requestManager.PostWithFileRequest(
    "v1/documents",
    jsonMetadata,
    new ByteArrayContent(fileBytes),
    "report.pdf");

// PUT with JSON body
var putRequest = _requestManager.PutRequest("v1/products/42", jsonPayload);
```

`RequestManager` accepts either valid relative URIs (resolved against `HttpClient.BaseAddress`) or valid absolute `http`/`https` URIs. Protocol-relative URLs, malformed URIs, unencoded whitespace, and unsafe upload file names are rejected with `ArgumentException`.

### Processing Requests

Inject `IRequestProcessor` to execute calls. All methods return `Task<Tuple<HttpStatusCode, string>>` (except file responses which return `HttpResponseMessage` for stream access):

```csharp
var client  = _clientBuilder.CreateOAuthClient("MyApi", baseUrl, token);
var request = _requestManager.GetRequest("v1/products");

var (statusCode, body) = await _requestProcessor.GetAsync(client, request);

if (statusCode == HttpStatusCode.OK)
{
    var products = JsonSerializer.Deserialize<List<Product>>(body);
}
```

Body-reading methods enforce a 10 MB response limit even when the server omits `Content-Length` and streams a chunked response. For larger payloads, use the file/streaming methods.

The library emits only minimal structured logs that are appropriate for a reusable class library: debug logs around client creation and request dispatch, plus warning logs when response-size limits reject a payload. Access tokens are never logged.

File download example:

```csharp
using var response = await _requestProcessor.GetFileAsync(client, request);
response.EnsureSuccessStatusCode();
var stream = await response.Content.ReadAsStreamAsync();
```

---

## Class Reference

### `HttpClientBuilder`

| Method | Parameters | Returns | Description |
|---|---|---|---|
| `CreateBasicClient` | `clientType`, `basePath` | `HttpClient` | `Accept: application/json` client |
| `CreateOAuthClient` | `clientType`, `basePath`, `accessToken` | `HttpClient` | Bearer auth + `Accept: application/json` |
| `CreateOAuthClientWithFile` | `clientType`, `basePath`, `accessToken` | `HttpClient` | Bearer auth, no Accept header (for file upload) |

### `RequestManager`

| Method | Parameters | Returns | Description |
|---|---|---|---|
| `GetRequest` | `url` | `HttpRequestMessage` | HTTP GET message |
| `PostRequest` | `url`, `content` | `HttpRequestMessage` | HTTP POST with JSON body |
| `PostRequest` | `url`, `MultipartFormDataContent` | `HttpRequestMessage` | HTTP POST with multipart body |
| `PostWithFileRequest` | `url`, `jsonString`, `ByteArrayContent`, `fileName` | `HttpRequestMessage` | HTTP POST with file + JSON metadata |
| `PutRequest` | `url`, `content` | `HttpRequestMessage` | HTTP PUT with JSON body |

### `RequestProcessor`

| Method | Returns | Description |
|---|---|---|
| `GetAsync` | `Task<Tuple<HttpStatusCode, string>>` | Executes GET |
| `PostAsync` | `Task<Tuple<HttpStatusCode, string>>` | Executes POST |
| `PutAsync` | `Task<Tuple<HttpStatusCode, string>>` | Executes PUT |
| `DeleteAsync` | `Task<Tuple<HttpStatusCode, string>>` | Executes DELETE |
| `GetFileAsync` | `Task<HttpResponseMessage>` | Executes GET, returns raw response for streaming |
| `PostFileRequestAsync` | `Task<HttpResponseMessage>` | Executes POST, returns raw response for streaming |

---

## Security Considerations

- **Never log access tokens.** Pass tokens at call time; do not cache them in the library.
- **Do not store tokens in static fields or singletons.** Use scoped services and short-lived clients.
- **OAuth clients require `https://` base paths.** The library enforces this and rejects malformed Bearer token header values with safe `ArgumentException` messages.
- **Use the streaming methods for large payloads.** `GetAsync`, `PostAsync`, `PutAsync`, and `DeleteAsync` are intentionally capped at 10 MB.
- **Rotate tokens before expiry.** The library passes tokens directly to `Authorization` headers — token refresh is the caller's responsibility.

See [SECURITY.md](SECURITY.md) for the full security policy.

---

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for development setup, coding standards, branch strategy, and pull request guidelines.

---

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for a full version history.


