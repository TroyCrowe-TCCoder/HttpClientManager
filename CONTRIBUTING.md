# Contributing to HttpClientManager

Thank you for contributing to **HttpClientManager**. This document describes the development environment setup, coding standards, branch strategy, and pull request process that all contributors are expected to follow.

---

## Table of Contents

- [Development Environment](#development-environment)
- [Project Structure](#project-structure)
- [Coding Standards](#coding-standards)
- [SOLID Principles](#solid-principles)
- [Branch Strategy](#branch-strategy)
- [Commit Message Format](#commit-message-format)
- [Pull Request Process](#pull-request-process)
- [Testing Requirements](#testing-requirements)
- [Definition of Done](#definition-of-done)

---

## Development Environment

| Tool | Version |
|---|---|
| .NET SDK | 9.0 |
| C# | 13.0 |
| IDE | Visual Studio 2026+ or VS Code with C# Dev Kit |

**Setup steps:**

```powershell
# Clone the repository
git clone https://dev.azure.com/tcrowe0170/_git/HttpClientManager
cd HttpClientManager

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run tests
dotnet test
```

---

## Project Structure

```
HttpClientManager/
├── Interfaces/                  # All public interface contracts
│   ├── IHttpClientBuilder.cs
│   ├── IRequestManager.cs
│   └── IRequestProcessor.cs
├── HttpClientBuilder.cs         # Creates configured HttpClient instances
├── RequestManager.cs            # Builds HttpRequestMessage objects
├── RequestProcessor.cs          # Executes HTTP calls
├── HostBuilder.cs               # DI registration helper
├── HttpClientManager.csproj
├── README.md
├── CONTRIBUTING.md              # This file
├── CHANGELOG.md
├── SECURITY.md
└── docs/
    └── ARCHITECTURE.md
```

---

## Coding Standards

This project targets **.NET 9 / C# 13** with `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`.

### Naming

- **Classes, Interfaces, Methods, Properties:** `PascalCase`
- **Private fields:** `_camelCase` (underscore prefix)
- **Parameters and locals:** `camelCase`
- **Async methods:** always suffix with `Async` (e.g., `GetProductAsync`)
- **Interfaces:** prefix with `I` followed by `PascalCase` (e.g., `IRequestManager`)

### Code Style

- Use **file-scoped namespaces**: `namespace HttpClientManager;`
- Prefer **implicit types** (`var`) when the type is obvious from the right-hand side
- Use **target-typed `new`** where the type is already stated: `HttpRequestMessage request = new(...)`
- Use **expression body** for single-line members
- Enable and respect **nullable reference types** — annotate all public API signatures
- Add **XML doc comments** (`///`) on all public types and members
- Keep methods short and focused — if a method exceeds ~20 lines, consider splitting it
- No magic strings — extract constants or use `nameof`

### Async

- All I/O methods must be `async Task` or `async Task<T>` — no synchronous wrappers
- Accept `CancellationToken` in every public async method signature and pass it through
- Use `ConfigureAwait(false)` in all library code
- Never use `.Result` or `.Wait()` — these cause deadlocks in async contexts

### Error Handling

- Use `ArgumentNullException.ThrowIfNull(param)` for null guards
- Use `string.IsNullOrWhiteSpace(s)` for string guards — throw `ArgumentException` with a helpful message
- Convert malformed caller input into precise public exceptions as early as possible; do not let invalid URLs, headers, or filenames escape later as framework `FormatException` / `UriFormatException`
- Never catch and swallow exceptions silently
- Do not catch base `Exception` unless logging and rethrowing

### Dependencies

- Do not add new NuGet packages without prior discussion in the pull request
- Do not upgrade existing packages without a corresponding `CHANGELOG.md` entry

---

## SOLID Principles

All changes must adhere to SOLID principles. Reviewers will reject contributions that violate them.

| Principle | Expectation |
|---|---|
| **SRP** | Each class has exactly one reason to change. `HttpClientBuilder` configures clients; `RequestManager` builds messages; `RequestProcessor` executes calls. Keep them separate. |
| **OCP** | Add new behaviour by implementing an existing interface or adding a new one — do not modify stable, tested code to add unrelated features. |
| **LSP** | All implementations must satisfy their interface contract fully. Do not implement a method to throw `NotImplementedException`. |
| **ISP** | Keep interfaces narrow. If a new interface would force consumers to depend on methods they don't use, split it. |
| **DIP** | All cross-component dependencies must be injected via constructor. No `new ConcreteType()` inside a class that could receive the dependency from outside. |

---

## Branch Strategy

This repository uses a **feature branch workflow** against `master`.

| Branch type | Pattern | Purpose |
|---|---|---|
| Feature | `feature/<short-description>` | New functionality |
| Bug fix | `fix/<short-description>` | Corrects a defect |
| Refactor | `refactor/<short-description>` | No behaviour change |
| Documentation | `docs/<short-description>` | Docs only |
| Release | `release/<semver>` | Release preparation |

**Rules:**
- Branch off `master` for all work
- Keep branches short-lived (ideally < 5 business days)
- Rebase onto `master` before raising a PR — no merge commits in feature branches
- Delete branches after merge

---

## Commit Message Format

Follow the [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
<type>(<scope>): <short summary>

[optional body]

[optional footer(s)]
```

**Types:**

| Type | When to use |
|---|---|
| `feat` | New feature or capability |
| `fix` | Bug fix |
| `refactor` | Code change that neither fixes a bug nor adds a feature |
| `test` | Adding or updating tests |
| `docs` | Documentation only |
| `chore` | Build, dependency, or tooling changes |
| `perf` | Performance improvement |

**Examples:**

```
feat(RequestProcessor): add CancellationToken support to all async methods

fix(HttpClientBuilder): guard against null or empty basePath

docs(README): add named client registration example
```

---

## Pull Request Process

1. **Open a PR** against `master` with a clear title using the Conventional Commits format
2. **Link the work item** in the PR description (Azure DevOps `AB#<id>`)
3. Fill in the PR template (below) completely
4. Ensure the build pipeline passes before requesting review
5. At least **one approving review** is required before merge
6. The PR author merges after approval — do not merge someone else's PR without permission

### PR Description Template

```markdown
## Summary
<!-- What does this PR do? -->

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Refactor
- [ ] Documentation
- [ ] Dependency update

## Related Work Item
AB#

## Testing
<!-- How was this tested? -->

## Checklist
- [ ] Code follows project coding standards
- [ ] XML doc comments added/updated for all public members
- [ ] Tests added or updated for all changed behaviour
- [ ] CHANGELOG.md updated
- [ ] No new compiler warnings introduced
- [ ] Nullable annotations are correct
```

---

## Testing Requirements

- All new public methods **must** have corresponding unit tests
- Tests live in a separate project: `HttpClientManager.Tests`
- Use the same test framework already present in the solution
- Follow the **Arrange-Act-Assert** pattern
- Name tests descriptively: `WhenBasepathIsNull_CreateBasicClient_ThrowsArgumentException`
- Test one behaviour per test method — no branching or conditionals inside tests
- Mock only external dependencies (e.g., `IHttpClientFactory`) — never mock the code under test

```powershell
# Run all tests
dotnet test

# Run with code coverage
dotnet-coverage collect -f cobertura -o coverage.cobertura.xml dotnet test
```

---

## Definition of Done

A change is considered **done** when all of the following are true:

- [ ] Code compiles with zero warnings (`dotnet build -warnaserror`)
- [ ] All existing tests pass
- [ ] New tests written and passing for changed/added behaviour
- [ ] Public API has XML doc comments
- [ ] `CHANGELOG.md` updated under `[Unreleased]`
- [ ] PR approved and merged to `master`
- [ ] Feature branch deleted
