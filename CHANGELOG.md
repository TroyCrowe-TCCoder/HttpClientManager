# Changelog

All notable changes to **HttpClientManager** are documented in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Added

- **`IRequestManager.PatchRequest` / `RequestManager.PatchRequest`** — new method that builds an HTTP PATCH message with a UTF-8 JSON body, following the same input validation and scheme-check pattern as `PostRequest` and `PutRequest`
- **`IRequestProcessor.PatchAsync` / `RequestProcessor.PatchAsync`** — new method that executes an HTTP PATCH request and returns an `HttpOperationResult`; enforces the 10 MB response-body limit and propagates `CancellationToken` end-to-end
- **`HttpOperationResult` record** — replaces `Tuple<HttpStatusCode, string>` as the return type for all body-reading processor methods (`GetAsync`, `PostAsync`, `PutAsync`, `PatchAsync`, `DeleteAsync`); exposes `StatusCode`, `Body`, and `ResponseHeaders` so callers can inspect server-returned headers (e.g. `ETag`, `Location`, `Retry-After`) without needing direct access to the `HttpResponseMessage`
- **`ServiceCollectionExtensions.AddHttpClientManager()`** — new `IServiceCollection` extension method that registers all HttpClientManager services into an existing collection; enables idiomatic `builder.Services.AddHttpClientManager()` registration in ASP.NET Core and Generic Host applications

### Breaking Changes

- **`IRequestProcessor` return types changed** — `GetAsync`, `PostAsync`, `PutAsync`, `DeleteAsync` now return `Task<HttpOperationResult>` instead of `Task<Tuple<HttpStatusCode, string>>`; update call sites from `result.Item1` / `result.Item2` to `result.StatusCode` / `result.Body`
- **`HostBuilder` removed** — `HostBuilder.ConfigureServices()` has been removed; replace with `services.AddHttpClientManager()` in your host setup

### Security

- **Chunked-response body reads are now bounded in-library** — `RequestProcessor` replaced `ReadAsStringAsync` with a streaming helper that enforces the 10 MB limit while reading the body; oversized chunked responses can no longer bypass the size limit when `Content-Length` is absent
- **Malformed relative URIs now fail early** — `RequestManager.EnsurePermittedScheme` now requires every `url` to be either a valid relative URI or a valid absolute `http`/`https` URI, preventing late `UriFormatException` failures during `HttpRequestMessage` construction
- **Safe Bearer header construction is now unconditional** — `HttpClientBuilder` now constructs `AuthenticationHeaderValue` before creating the `HttpClient` and wraps any remaining `FormatException` as `ArgumentException`, preventing raw token values from escaping in exception messages and avoiding abandoned partially configured clients
- **Full ASCII control-character rejection for upload file names** — `RequestManager.EnsureSafeFileName` now rejects all C0 control characters (`0x00–0x1F`) and DEL (`0x7F`) in addition to `"`, `\`, `/`, and `:`; this prevents `Content-Disposition` corruption and late `FormatException` escapes from header construction
- **Quoted charset values are handled safely** — `RequestProcessor` now trims quoted `Content-Type` charset values before resolving `Encoding` and only catches the specific invalid/unsupported-encoding exceptions needed for UTF-8 fallback
- **Bearer token character validation** — `HttpClientBuilder.EnsureSafeBearerToken` now validates `accessToken` before `AuthenticationHeaderValue` is constructed; `AuthenticationHeaderValue` throws `FormatException` on illegal characters and includes the raw token value in its exception message (credential exposure in structured logs); the new guard throws `ArgumentException` with a safe message; CR/LF in a token is also a header-injection vector
- **`BaseAddress` trailing-slash normalisation** — `HttpClientBuilder.ParseAbsoluteUri` and `ParseAndEnsureHttpsUri` now append a trailing `/` to the resolved URI path when absent; without it, `new Uri(baseAddress, relativeUrl)` silently drops any path segment on the base (e.g. `new Uri(new Uri("https://host/v1"), "users")` → `https://host/users`), causing requests to bypass the intended API root
- **URL whitespace validation** — `RequestManager.EnsurePermittedScheme` now rejects URLs containing unencoded space, tab, CR, or LF; a whitespace-prefixed absolute URL fails `Uri.TryCreate(Absolute)` and is misclassified as a relative URI, bypassing scheme validation entirely and producing `UriFormatException` at dispatch instead of the library's `ArgumentException`
- **OOM protection timing fixed** — `RequestProcessor` body-reading methods now use `HttpCompletionOption.ResponseHeadersRead` for all `SendAsync` calls; the `Content-Length` guard previously ran *after* the full body was already buffered (default `ResponseContentRead` behaviour), making it completely ineffective; it now runs *before* any body bytes are read
- **Chunked-encoding response size limit** — `HostBuilder.ConfigureServices` now calls `ConfigureHttpClientDefaults` to set `MaxResponseContentBufferSize = 10 MB` on all created `HttpClient` instances; this closes the second half of the OOM surface that `Content-Length`-based checking cannot cover (chunked responses carry no `Content-Length` header)
- **`:` blocked in file names** — `RequestManager.EnsureSafeFileName` now rejects `:` in addition to `"`, `\`, `/`, CR, LF, and null; `:` enables NTFS Alternate Data Streams (`report.pdf:payload.exe`) and Windows drive separators when a server stores the filename naively
- **NuGet Audit enforced in project file** — `NuGetAudit`, `NuGetAuditLevel=moderate`, and `NuGetAuditMode=all` added to `HttpClientManager.csproj`; known-vulnerability scanning now runs on every `dotnet restore` / `dotnet build` rather than being documentation-only guidance
- **`CreateBasicClient` scheme restriction** — `HttpClientBuilder.ParseAbsoluteUri` now restricts `basePath` to `http`/`https` schemes; previously `ftp://`, `file://`, and any other absolute URI was accepted as `HttpClient.BaseAddress`, creating an SSRF path where a relative request URL (validated by `RequestManager`) would resolve against a non-HTTP base
- **Response body size limit (Content-Length)** — `RequestProcessor` body-reading methods enforce a 10 MB `Content-Length` ceiling; exceeding it throws `InvalidOperationException` and directs callers to the streaming methods
- **Multipart form-field `name` fixed** — `RequestManager.PostWithFileRequest` now uses the constant `"file"` as the multipart `Content-Disposition` `name` parameter instead of the caller-supplied `fileName`; previously user input controlled the form-field control name as well as the filename
- **`/` blocked in file names** — `RequestManager.EnsureSafeFileName` now rejects `/` in addition to `"`, `\`, CR, LF, and null, preventing server-side path traversal when the receiving server stores the file by its declared name
- **Credential echo in error messages eliminated** — `HttpClientBuilder.ParseAbsoluteUri` no longer includes the raw `basePath` value in exception messages; URIs can contain embedded credentials (`user:password@host`) or tokens (`?api_key=...`) that would otherwise appear in structured logs and APM traces
- **Protocol-relative URL injection blocked** — `RequestManager` now explicitly rejects URLs beginning with `//` (e.g. `//evil.com/path`) with `ArgumentException`; previously `Uri.TryCreate` classified these as relative URIs, silently bypassing the URI scheme check and enabling SSRF
- **`CreateBasicClient` URI validation** — `HttpClientBuilder.CreateBasicClient` now throws `ArgumentException` (not `UriFormatException`) for malformed or non-absolute `basePath` values, giving a consistent public exception contract across all three factory methods
- **HTTPS enforcement on OAuth clients** — `HttpClientBuilder.CreateOAuthClient` and `CreateOAuthClientWithFile` now throw `ArgumentException` when `basePath` is not an `https://` URI, preventing Bearer tokens from being transmitted in plaintext
- **Header drop fixed in `GetAsync`, `DeleteAsync`, `PutAsync`** — all `RequestProcessor` methods now use `client.SendAsync(request)` so that custom security headers set on the `HttpRequestMessage` (e.g. `X-CSRF-Token`, per-request `Authorization`) are preserved and sent; previously they were silently discarded
- **`HttpResponseMessage` disposal** — body-reading methods (`GetAsync`, `PostAsync`, `PutAsync`, `DeleteAsync`) now dispose the response via `using var`, preventing TCP connection exhaustion under concurrent load
- **Content-Disposition header injection blocked** — `RequestManager.PostWithFileRequest` rejects `fileName` values containing `"`, `\`, `\r`, `\n`, or `\0`
- **URI scheme validation** — all `RequestManager` methods now reject absolute URIs with non-HTTP(S) schemes (e.g. `file://`, `ftp://`) to prevent SSRF via unexpected transport protocols; relative URIs are permitted and continue to resolve against `HttpClient.BaseAddress`
- **File streaming via `ResponseHeadersRead`** — `GetFileAsync` and `PostFileRequestAsync` now use `HttpCompletionOption.ResponseHeadersRead`, allowing callers to stream large files without fully buffering the response body

### Breaking Changes

- **`IhttpClientBuilder` renamed to `IHttpClientBuilder`** — corrects PascalCase violation; update all injection and reference sites
- **`IRequestProcessor` methods renamed with `Async` suffix** — `Get` → `GetAsync`, `Post` → `PostAsync`, `Put` → `PutAsync`, `Delete` → `DeleteAsync`, `GetFile` → `GetFileAsync`, `PostFileRequest` → `PostFileRequestAsync`; update all call sites
- **`HostBuilder.ConfigureSerices()` renamed to `ConfigureServices()`** — corrects typo; update all call sites
- **`IRequestProcessor.IHttpClientBuilder` source file renamed** — `Interfaces/IhttpClientBuilder.cs` → `Interfaces/IHttpClientBuilder.cs`

### Added

- `CancellationToken ct = default` added to all six `IRequestProcessor` / `RequestProcessor` methods (ADR-004 implemented)
- Null/whitespace guards on all public method parameters:
  - `ArgumentException.ThrowIfNullOrWhiteSpace` on all `string` parameters in `HttpClientBuilder` and `RequestManager`
  - `ArgumentNullException.ThrowIfNull` on `client` and `request` parameters in all `RequestProcessor` methods
  - `ArgumentNullException.ThrowIfNull` on `IHttpClientFactory` constructor parameter in `HttpClientBuilder`
- XML doc comments (`///`) on all public types and members across all files
- `IHttpClientBuilder` registration added to `HostBuilder.ConfigureServices()` (was previously missing)

### Changed

- All `.cs` files migrated to **file-scoped namespaces** (`namespace HttpClientManager;`)
- `HttpClientBuilder` refactored to use a **primary constructor** (C# 13)
- `RequestManager` — redundant intermediate variables removed; **target-typed `new()`** used throughout
- `RequestProcessor.GetFileAsync` and `PostFileRequestAsync` — removed pointless `async`/`await` wrapper; methods now return the `Task` directly (`ConfigureAwait` not required on non-async path)
- `ConfigureAwait(false)` added to all `await` expressions in `RequestProcessor` (library code must not capture synchronisation context)
- `HostBuilder.ConfigureServices()` — removed duplicate `AddHttpClient()` call and no-op `BuildServiceProvider()` call; registration uses generic `AddScoped<TService, TImpl>()` instead of `typeof()` overloads

### Removed

- Redundant `using` directives in `RequestProcessor` (`System`, `System.Net.Http`, `System.Threading.Tasks` — all provided by implicit usings in SDK)
- Unused `using System.IO` in `RequestManager`

---

## [1.0.0] - 2025-07-11

### Added

- `IhttpClientBuilder` / `HttpClientBuilder` — factory for creating configured `HttpClient` instances
  - `CreateBasicClient` — sets `BaseAddress` and `Accept: application/json`
  - `CreateOAuthClient` — adds `Authorization: Bearer <token>` and `Accept: application/json`
  - `CreateOAuthClientWithFile` — adds `Authorization: Bearer <token>` without an Accept header (multipart file uploads)
- `IRequestManager` / `RequestManager` — builder for `HttpRequestMessage` objects
  - `GetRequest` — HTTP GET message
  - `PostRequest(url, jsonString)` — HTTP POST with `application/json` body
  - `PostRequest(url, MultipartFormDataContent)` — HTTP POST with multipart body
  - `PostWithFileRequest` — HTTP POST with combined JSON metadata and file `ByteArrayContent`
  - `PutRequest` — HTTP PUT with `application/json` body
- `IRequestProcessor` / `RequestProcessor` — executor that sends requests and normalises responses
  - `Get`, `Post`, `Put`, `Delete` — return `Task<Tuple<HttpStatusCode, string>>`
  - `GetFile`, `PostFileRequest` — return raw `Task<HttpResponseMessage>` for stream access
- `HostBuilder.ConfigureSerices()` — static `IServiceCollection` registration helper
- Full interface segregation across three focused interfaces (SRP / ISP)
- .NET 9 target framework with nullable reference types enabled

---

[Unreleased]: https://dev.azure.com/tcrowe0170/_git/HttpClientManager/branchCompare?baseVersion=GTv1.0.0&targetVersion=GBmaster
[1.0.0]: https://dev.azure.com/tcrowe0170/_git/HttpClientManager/tags
