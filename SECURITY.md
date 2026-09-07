# Security Policy

## Supported Versions

Only the latest release of **HttpClientManager** receives security fixes. Older versions are not patched.

| Version | Supported |
|---|---|
| 2.x (latest) | ✅ Yes |
| < 2.0 | ❌ No |

---

## Reporting a Vulnerability

**Do not open a public issue for security vulnerabilities.**

Report security concerns privately via GitHub Security Advisories at:

```
https://github.com/TroyCrowe-TCCoder/HttpClientManager/security/advisories/new
```

Include as much detail as possible:

- A description of the vulnerability
- Steps to reproduce (proof-of-concept code if applicable)
- The potential impact and affected versions
- Any suggested mitigation

You will receive an acknowledgment within **5 business days** and a resolution timeline within **15 business days** of triage.

---

## Security Design Considerations

HttpClientManager is a class library. The security of any deployment depends heavily on how consuming applications use it. The following guidance applies to both this library and its consumers.

### Library-Enforced Controls

The following defences are implemented directly in the library and cannot be bypassed by callers. Input-validation failures throw `ArgumentException`; oversized response bodies are rejected with `InvalidOperationException`:

| Control | Where enforced | What it prevents |
|---|---|---|
| HTTPS-only base path | `HttpClientBuilder.CreateOAuthClient`, `CreateOAuthClientWithFile` | Bearer tokens transmitted in plaintext over HTTP |
| Absolute URI validation (http/https only) | `HttpClientBuilder` — all methods | `ArgumentException` on malformed `basePath` or non-HTTP(S) schemes (`ftp://`, `file://`) set as `HttpClient.BaseAddress` |
| Permitted URI schemes (`http`, `https` only) | `RequestManager` — all methods | SSRF via `file://`, `ftp://`, or other dangerous URI schemes in request URLs |
| Protocol-relative URL rejection (`//host/path`) | `RequestManager` — all methods | SSRF bypass where `Uri.TryCreate` treats `//host/path` as a relative URI, silently passing scheme validation |
| Safe `Content-Disposition` filename | `RequestManager.PostWithFileRequest` | Header injection via ASCII control characters / DEL / `"` / `\`; path traversal via `/`; NTFS Alternate Data Streams and drive separators via `:` |
| Fixed multipart form-field `name` parameter | `RequestManager.PostWithFileRequest` | User input cannot control the `name` attribute in `Content-Disposition: form-data; name="..."` |
| Response body size limit — declared (10 MB) | `RequestProcessor` — all body-reading methods | OOM via oversized `Content-Length`; check fires **before** body is buffered (`ResponseHeadersRead` used on all methods) |
| Response body size limit — chunked (10 MB) | `RequestProcessor` — all body-reading methods | OOM via chunked-encoding responses that carry no `Content-Length` header; the body is streamed with an in-library 10 MB cap |
| Null/whitespace guards | All public method parameters | `NullReferenceException` and empty-string bypass of downstream validation |
| Bearer token header validation | `HttpClientBuilder.CreateOAuthClient`, `CreateOAuthClientWithFile` | `FormatException` with raw token value in logs (credential exposure); malformed `Authorization` header values; header injection via CR/LF in `Authorization` header |
| `BaseAddress` trailing-slash normalisation | `HttpClientBuilder` — both URI helpers | Silent path-segment loss when combining base URI with relative request URI (RFC 3986 §5.2.2) — e.g. `/v1` dropped from `https://host/v1` |
| URL whitespace validation | `RequestManager` — all methods | Unencoded whitespace causes scheme validation bypass (whitespace-prefixed absolute URL fails `Uri.TryCreate(Absolute)` and is misclassified as relative) |
| Relative URI validation | `RequestManager` — all methods | Malformed non-absolute URLs fail early with `ArgumentException` instead of escaping later as `UriFormatException` during request construction |
| Pre-construction validation before `CreateClient` | `HttpClientBuilder` — all methods | Prevents partially configured `HttpClient` instances from being created when URI or Bearer-header validation fails |

> **Note:** `CreateBasicClient` does not enforce HTTPS because unauthenticated clients are legitimately used against internal HTTP endpoints. Prefer HTTPS everywhere regardless.

### Access Token Handling

- **Never log access tokens.** Do not pass tokens to any logging framework, even at `Debug` level.
- **Do not cache tokens in static fields or singletons.** Services registered as `Scoped` are disposed per request scope; do not promote tokens to longer lifetimes.
- **Treat tokens as secrets.** Store them in a secrets manager (Azure Key Vault, environment variables, .NET Secret Manager) — never in source code or `appsettings.json` committed to source control.
- **Token refresh is the caller's responsibility.** This library passes the token provided at call time directly into the `Authorization` header. Expired tokens will result in `401 Unauthorized` responses.
- **Use short-lived tokens.** Prefer tokens with lifetimes of minutes over hours where the downstream API supports it.

### Transport Security

- **HTTPS is enforced by the library for all OAuth clients.** `CreateOAuthClient` and `CreateOAuthClientWithFile` throw `ArgumentException` if `basePath` is not an `https://` URI.
- **Do not disable certificate validation.** Never configure an `HttpClientHandler` with `ServerCertificateCustomValidationCallback` that returns `true` unconditionally — this disables TLS chain validation entirely.
- **Pin certificates only when required.** If certificate pinning is required, implement it as a named `HttpClient` policy in the consuming application, not inside this library.
- **Disable automatic redirects on OAuth clients.** `HttpClient` follows 301/302 redirects by default. A server redirect from `https://` to `http://` will transmit the Bearer token in plaintext on the redirected request — the library’s HTTPS enforcement only covers the initial `BaseAddress` and cannot intercept redirects. Configure `AllowAutoRedirect = false` on OAuth named clients:

```csharp
builder.Services.AddHttpClient("OAuthApi")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        AllowAutoRedirect = false
    });
```

### Named Client Configuration

When registering named clients with `IHttpClientFactory` in the consuming application:

```csharp
// DO: configure timeouts, response buffer limits, and disable redirects for OAuth clients
builder.Services.AddHttpClient("MyApi", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    // Defense in depth: the library already enforces a 10 MB limit for both declared and
    // chunked responses in its body-reading methods. Matching the client buffer limit keeps
    // behavior consistent if callers read content outside RequestProcessor.
    client.MaxResponseContentBufferSize = 10 * 1024 * 1024; // 10 MB
});

// DO: disable auto-redirects on OAuth named clients to prevent HTTPS→HTTP token downgrade
builder.Services.AddHttpClient("OAuthApi")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        AllowAutoRedirect = false
    });

// DO NOT: store credentials inside the named client factory configuration
// Credentials must be applied per-request via HttpClientBuilder methods
```

### Input Validation

- All `url` and `basePath` parameters are validated for permitted URI schemes by the library. Absolute URIs using schemes other than `http` or `https` are rejected with `ArgumentException`.
- Protocol-relative URLs (`//host/path`) are explicitly rejected in `RequestManager` with `ArgumentException`. They are not treated as relative URIs because doing so would silently bypass scheme validation.
- URLs containing unencoded whitespace (space, tab, CR, LF) are rejected with `ArgumentException`. A whitespace-prefixed absolute URL fails `Uri.TryCreate(UriKind.Absolute)` and is misclassified as a relative URI, bypassing scheme validation entirely.
- Malformed non-absolute URLs are also rejected with `ArgumentException`. `RequestManager` requires every `url` to be either a valid relative URI or a valid absolute `http`/`https` URI, preventing late `UriFormatException` failures in `HttpRequestMessage`.
- Malformed or non-absolute `basePath` values are rejected with `ArgumentException` in all `HttpClientBuilder` methods — callers will always receive a consistent exception type. Error messages do not echo the raw `basePath` value to prevent credentials embedded in URIs (`user:password@host`, `?api_key=secret`) from appearing in logs.
- `accessToken` values are validated for forbidden characters before `AuthenticationHeaderValue` is constructed. If `AuthenticationHeaderValue` still rejects a malformed token (for example due to RFC token separator characters), the library catches `FormatException` and rethrows a safe `ArgumentException` so the raw token value never appears in logs. CR/LF in a token is also a header-injection vector.
- `BaseAddress` URIs are normalised to include a trailing `/` by both `HttpClientBuilder` URI helpers. Without it, `new Uri(baseAddress, relativeUrl)` silently drops the last path segment from the base (e.g. `new Uri(new Uri("https://host/v1"), "users")` → `https://host/users`), causing all requests to bypass the intended API root.
- `fileName` values in `PostWithFileRequest` reject all ASCII control characters (`0x00–0x1F`), DEL (`0x7F`), `"`, `\`, `/`, and `:`. This prevents header corruption, legacy folded-header injection, server-side path traversal, and NTFS Alternate Data Streams.
- **SSRF to internal services:** This library does not block requests to private IP ranges (`10.x`, `172.16– 31.x`, `192.168.x`, `169.254.x` link-local / cloud metadata). If the consuming application proxies untrusted user-supplied URLs, implement an IP-address allowlist/denylist at the `HttpMessageHandler` or network layer rather than in this library.

### Dependency Supply Chain

- **NuGet Audit is enforced** — `NuGetAudit`, `NuGetAuditLevel`, and `NuGetAuditMode` are set in `HttpClientManager.csproj`. Every `dotnet restore` and `dotnet build` checks all direct and transitive packages against the NuGet Advisory Database and fails the build on moderate-or-higher advisories.
- Review all transitive dependencies when updating `Microsoft.Extensions.Http`.

---

## Related Resources

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [Microsoft Security Development Lifecycle](https://www.microsoft.com/en-us/securityengineering/sdl)
- [.NET Security Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/security/security-best-practices)
- [Azure Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/general/overview)
