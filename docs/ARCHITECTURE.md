# Architecture — HttpClientManager

## Purpose

This document describes the component design, design patterns, and architectural decisions for **HttpClientManager**. It serves as the authoritative reference for understanding *why* the library is structured the way it is and *where* new features belong.

---

## Component Overview

```
┌─────────────────────────────────────────────────────────┐
│                   Consuming Application                 │
│            (front-end / orchestration layer)            │
└────────────────┬──────────────┬───────────────┬─────────┘
                 │              │               │
                 ▼              ▼               ▼
       IHttpClientBuilder  IRequestManager  IRequestProcessor
                 │              │               │
                 ▼              ▼               ▼
        HttpClientBuilder  RequestManager  RequestProcessor
                 │
                 ▼
         IHttpClientFactory
         (Microsoft.Extensions.Http)
```

### Layer Responsibilities

| Component | Interface | Responsibility | Must NOT |
|---|---|---|---|
| `HttpClientBuilder` | `IHttpClientBuilder` | Produce configured `HttpClient` instances | Store tokens, own lifecycle of clients |
| `RequestManager` | `IRequestManager` | Build `HttpRequestMessage` objects | Perform I/O, hold state |
| `RequestProcessor` | `IRequestProcessor` | Execute HTTP calls and return `HttpOperationResult` | Interpret business logic in responses |
| `ServiceCollectionExtensions` | — | Register services via `AddHttpClientManager()` | Be used outside application bootstrap |

---

## Design Patterns

### Factory Pattern — `HttpClientBuilder`

`HttpClientBuilder` acts as a **factory** for `HttpClient`. It delegates the actual construction to `IHttpClientFactory` (provided by the DI container) and applies auth headers and base address before returning the configured client. This isolates consuming code from the mechanics of `IHttpClientFactory` named client management.

It also consumes `ILogger<HttpClientBuilder>` for minimal debug-level diagnostics when a client has been created successfully. It does not log tokens, raw URIs, or validation failures that could expose secrets.

```
Consumer → IHttpClientBuilder.CreateOAuthClient(...)
                    ↓
         IHttpClientFactory.CreateClient(name)
                    ↓
         Apply headers (Authorization, Accept)
                    ↓
         Return configured HttpClient
```

### Builder Pattern — `RequestManager`

`RequestManager` implements a lightweight **builder** approach for `HttpRequestMessage`. Each method corresponds to a specific HTTP method and payload shape. The caller selects the appropriate method and receives a ready-to-use message object, keeping the construction of `HttpRequestMessage` out of application code.

### Strategy Pattern — `IRequestProcessor`

`RequestProcessor` applies a **strategy** per HTTP verb. Consumers select the correct method (`GetAsync`, `PostAsync`, `PatchAsync`, `PutAsync`, `DeleteAsync`, `GetFileAsync`, `PostFileRequestAsync`) based on the operation, and the processor handles the dispatch. Adding a new verb or response shape means adding a new method to the interface and implementation without changing existing methods (OCP).

`RequestProcessor` also consumes `ILogger<RequestProcessor>` because it is the library's network-I/O boundary. Logging is intentionally limited to request-dispatch debug messages and warning messages when the library rejects oversized responses.

### Service Locator avoided — Dependency Injection

No component calls `IServiceProvider` directly. All dependencies are constructor-injected. `ServiceCollectionExtensions.AddHttpClientManager()` is the single registration point, keeping the rest of the library free of DI-framework concerns.

`AddHttpClientManager()` registers `Microsoft.Extensions.Logging` and `IHttpClientFactory` so the library's `ILogger<T>` and `IHttpClientFactory` dependencies resolve correctly inside any host.

---

## Data Flow

### Standard Request (JSON response)

```
1. Caller obtains HttpClient
      IHttpClientBuilder.CreateOAuthClient("Name", baseUrl, token)
      → HttpClient with BaseAddress + Authorization header

2. Caller builds request message
      IRequestManager.PostRequest("v1/resource", jsonBody)
      → HttpRequestMessage { Method=POST, Content=StringContent }

3. Caller executes request
      IRequestProcessor.PostAsync(client, request)
      → bounded body read (10 MB max for declared or chunked responses)
      → returns HttpOperationResult { StatusCode, Body, ResponseHeaders }

4. Caller deserializes body
      JsonSerializer.Deserialize<T>(body)
```

### File Download

```
1–2. Same as above (GetRequest)

3. IRequestProcessor.GetFileAsync(client, request)
      → returns HttpResponseMessage (not read)

4. Caller streams content
      response.Content.ReadAsStreamAsync()
```

---

## Dependency Graph

```
HttpClientManager.csproj
├── Microsoft.Extensions.Http        (IHttpClientFactory)
└── Microsoft.Extensions.DependencyInjection  (IServiceCollection)

HttpClientBuilder
├── IHttpClientFactory  (external — injected)
└── ILogger<HttpClientBuilder>  (external — injected)

RequestManager
└── (no dependencies — pure factory)

RequestProcessor
├── ILogger<RequestProcessor>  (external — injected)
└── receives HttpClient at call time

ServiceCollectionExtensions  (static — no dependencies)
└── registers into IServiceCollection
```

---

## Architectural Decision Records (ADRs)

### ADR-001: Return `HttpOperationResult` from processor methods

**Status:** Supersedes initial `Tuple<HttpStatusCode, string>` decision

**Context:** The processor needs to return the HTTP status code, response body, and response headers. The original `Tuple<HttpStatusCode, string>` provided no access to headers (e.g. `ETag`, `Location`, `Retry-After`), which is needed for correct REST interaction patterns.

**Decision:** Replace `Tuple<HttpStatusCode, string>` with the `HttpOperationResult` sealed record (`StatusCode`, `Body`, `ResponseHeaders`). Return `HttpResponseMessage` directly for file/streaming operations where the caller needs full control.

**Consequences:** Breaking change at the call site (`result.Item1`/`result.Item2` → `result.StatusCode`/`result.Body`). Callers must still deserialise the body themselves — the processor has no knowledge of target types (SRP). A future ADR may introduce a generic `GetAsync<T>` overload if strong typing is needed widely.

---

### ADR-002: Separate `CreateOAuthClientWithFile` from `CreateOAuthClient`

**Status:** Accepted

**Context:** File-upload endpoints typically require multipart content without an `Accept: application/json` header. Including the header caused some downstream APIs to reject requests.

**Decision:** Add a dedicated `CreateOAuthClientWithFile` factory method that omits the `Accept` header. The method name makes the intent explicit at the call site.

**Consequences:** Three factory methods exist on `IHttpClientBuilder`. Callers must consciously select the right one, which makes misuse visible at code review time.

---

### ADR-003: DI registration via `IServiceCollection` extension method

**Status:** Updated — `HostBuilder` removed

**Context:** The original `HostBuilder.ConfigureServices()` returned a new isolated `IServiceCollection`, which was incompatible with hosts that already owned a collection (ASP.NET Core, Generic Host). The idiomatic .NET pattern is a `this IServiceCollection` extension method.

**Decision:** `HostBuilder` was removed. `ServiceCollectionExtensions.AddHttpClientManager(this IServiceCollection)` is the sole registration entry point.

**Consequences:** Consumers call `builder.Services.AddHttpClientManager()` and the library registers into the host's existing container. `ServiceCollectionExtensions` is a static class and cannot be mocked; only the registered services are tested.

---

### ADR-004: `CancellationToken` propagation

**Status:** Implemented

**Context:** The initial version of `RequestProcessor` did not propagate `CancellationToken` through to `HttpClient` calls, meaning long-running requests could not be cancelled by the caller.

**Decision:** Added `CancellationToken ct = default` to all six methods in `IRequestProcessor` and their `RequestProcessor` implementations. The token is forwarded to every downstream `HttpClient` call and to the bounded response-body reader used by the JSON/string-returning methods. All async methods in `IRequestProcessor` were also renamed with the `Async` suffix to comply with the async naming standard.

**Consequences:** Non-breaking API addition (default parameter). Existing callers require no changes. Callers that need cooperative cancellation now pass a `CancellationToken` from their own scope. The body-reading methods also enforce the library's 10 MB response-size limit during streaming, so chunked responses no longer rely solely on named-client buffer configuration.

---

---

### ADR-005: PATCH verb added to `IRequestManager` and `IRequestProcessor`

**Status:** Accepted

**Context:** REST APIs (including Microsoft Graph and Azure REST APIs) use PATCH for partial updates. The library previously supported only GET, POST, PUT, and DELETE, requiring consumers to build `HttpRequestMessage` manually for PATCH operations.

**Decision:** Added `IRequestManager.PatchRequest(url, content)` and `IRequestProcessor.PatchAsync(client, request, ct)`, following the same validation, encoding, and size-limit patterns as the existing verbs.

**Consequences:** The PATCH surface is consistent with the existing verb methods. Consumers that previously constructed PATCH messages manually can migrate to the new methods.

---

## Future Considerations

| Topic | Notes |
|---|---|
| Resilience (Polly) | Retry and circuit-breaker policies should be configured on named `HttpClient` registrations in the consuming app, not inside this library |
| Response deserialization | A generic `IRequestProcessor.GetAsync<T>` overload could reduce caller boilerplate — evaluate when a second consumer exists |
| Telemetry | `IHttpClientFactory` supports `DelegatingHandler` — an optional tracing handler could be added without breaking existing registrations |
| NuGet packaging | Package as a NuGet library when shared across multiple solution repositories |
