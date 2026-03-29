# GitHub Copilot Instructions — HttpClientManager

## Project Identity

**HttpClientManager** is a **.NET 9 / C# 13 class library** that wraps `IHttpClientFactory` and provides a clean, DI-friendly abstraction for consuming HTTP APIs. It is consumed by front-end or orchestration applications. The library lives in an **Azure DevOps** repository.

Act as an expert .NET software architect and pair programmer who is strict about code quality, SOLID principles, and security.

---

## Technology Stack

| Concern | Choice |
|---|---|
| Target framework | `net9.0` |
| Language version | C# 13 |
| Nullable | enabled |
| Implicit usings | enabled |
| HTTP | `IHttpClientFactory` via `Microsoft.Extensions.Http` |
| DI | `Microsoft.Extensions.DependencyInjection` |
| Testing | xUnit (preferred) |

---

## Architecture Rules

This library uses **three focused layers**, each backed by an interface. Do not merge them.

| Layer | Interface | Implementation | Single Responsibility |
|---|---|---|---|
| Client factory | `IHttpClientBuilder` | `HttpClientBuilder` | Create configured `HttpClient` instances |
| Request builder | `IRequestManager` | `RequestManager` | Build `HttpRequestMessage` objects |
| Request executor | `IRequestProcessor` | `RequestProcessor` | Send requests and return normalised responses |

- `HostBuilder` provides the DI registration convenience method.
- **`IHttpClientBuilder` lives in `HttpClientManager.Interfaces`** — do not confuse it with `Microsoft.Extensions.DependencyInjection.IHttpClientBuilder`. Use the `Interfaces.` prefix or a `using` alias when both namespaces are in scope.
- **Never add static state, singletons holding tokens, or I/O inside `RequestManager`.** It is a pure message factory.
- **Never add business logic inside `RequestProcessor`.** It only dispatches and returns; it does not interpret responses.

---

## SOLID Enforcement

### Single Responsibility Principle
Each class has one reason to change. When asked to add a feature:
- New client auth scheme → extend `HttpClientBuilder` / `IHttpClientBuilder`
- New HTTP verb or request shape → extend `RequestManager` / `IRequestManager`
- New response-handling behaviour → extend `RequestProcessor` / `IRequestProcessor`
- Never mix these responsibilities in one class

### Open/Closed Principle
Extend via new interface members or new implementations. Do not modify existing stable, tested methods to add unrelated behaviour.

### Liskov Substitution Principle
Every implementation must fully satisfy its interface contract. Do not generate `NotImplementedException` stubs.

### Interface Segregation Principle
Keep interfaces narrow and focused. If a proposed interface method doesn't belong to all consumers, create a separate interface.

### Dependency Inversion Principle
All cross-component dependencies must be injected via constructor. Do not `new` a concrete type inside a class that could receive it from the DI container.

---

## Code Style Directives

### Naming
- Classes, interfaces, methods, properties: `PascalCase`
- Private fields: `_camelCase`
- Parameters and locals: `camelCase`
- Async methods: always suffix `Async`
- Interfaces: `I` prefix + `PascalCase`

### Modern C# Preferences
- File-scoped namespaces: `namespace HttpClientManager;`
- Target-typed `new()` where type is already declared
- Expression-body members for single-expression methods
- `var` when the right-hand side makes the type obvious
- Raw string literals `"""` for multi-line strings
- Primary constructors where applicable in .NET 9

### Nullable Reference Types
- All public API signatures must have correct nullable annotations (`string?`, `string`)
- Guard public methods with `ArgumentNullException.ThrowIfNull(param)` for reference types
- Guard string params with `ArgumentException.ThrowIfNullOrWhiteSpace(param)`

### Async
- All I/O methods are `async Task` or `async Task<T>` — no sync-over-async
- Every public async method accepts a `CancellationToken ct = default` parameter
- Pass `CancellationToken` through to all downstream async calls
- Use `ConfigureAwait(false)` — this is a library, not an app
- No `.Result`, `.Wait()`, or `Task.Run` wrapping synchronous work

### Error Handling
- Prefer precise exception types: `ArgumentException`, `ArgumentNullException`, `InvalidOperationException`, `HttpRequestException`
- Never catch base `Exception` without logging and rethrowing
- No silent swallowing of exceptions

---

## Security Directives

- **Never log, store, or cache access tokens** in any generated code
- All `basePath` / `url` parameters should be documented as requiring `https://` URIs
- Do not disable TLS certificate validation in any generated `HttpClientHandler` configuration
- Do not add secrets, connection strings, or credentials to any file that could be committed to source control
- When generating test code, use placeholder/stub tokens — never real credentials

---

## Testing Directives

- Test project name: `HttpClientManager.Tests`
- Mirror source class names: `HttpClientBuilder` → `HttpClientBuilderTests`
- Use **xUnit** (`[Fact]`, `[Theory]`, `[InlineData]`)
- Follow **Arrange-Act-Assert** — one assertion per `[Fact]` where possible
- Test method names: `WhenCondition_Method_ExpectedOutcome` (e.g., `WhenBasePathIsNull_CreateBasicClient_ThrowsArgumentException`)
- Mock only external dependencies (`IHttpClientFactory`) — never mock the system under test
- No disk I/O in unit tests
- Tests must be runnable in any order

---

## Documentation Updates
- When code changes require documentation updates, always update the relevant markdown files as part of the work.

---

## Prohibited Actions

- Do not generate code that violates any SOLID principle
- Do not add a new NuGet package without noting it in the response and `CHANGELOG.md`
- Do not change `<TargetFramework>`, `<LangVersion>`, or `<Nullable>` settings
- Do not modify auto-generated files (`*.g.cs`, `AssemblyInfo.cs`, files under `obj/`)
- Do not add `public` visibility unless it is part of the public API surface
- Do not use `dynamic`, `object` parameters, or untyped collections in new code
- Do not provide vague responses — be specific with code examples and rationale

---

## Response Style

- Be concise and direct
- When proposing changes, explain *why* (the principle or security concern) in one sentence
- Show before/after diffs for non-trivial changes
- Flag any deviation from the directives above with a `> ⚠️ Note:` block
