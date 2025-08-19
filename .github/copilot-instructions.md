# Microsoft Omex Extensions - Copilot Instructions

## Project Overview

This is a collection of shared .NET extensions and utilities for building scalable, distributed systems at Microsoft. The codebase follows a modular architecture with multiple NuGet packages, each serving specific purposes in service orchestration, telemetry, health monitoring, and Service Fabric integration.

## Architecture & Module Structure

### Core Modules

- **Abstractions**: Base interfaces and utilities (`IAccessor<T>`, validation, tagging)
- **Activities**: OpenTelemetry/activity management and metrics collection
- **Hosting**: Service hosting utilities with environment-specific configurations (`OmexEnvironments`)
- **Logging**: Centralized logging infrastructure with scrubbing capabilities
- **Diagnostics.HealthChecks**: Composable health check framework using decorator pattern
- **ServiceFabricGuest.Abstractions**: Service Fabric client wrappers and abstractions
- **Services.Remoting**: Service Fabric remoting extensions

### Key Patterns

#### Service Registration Pattern

Extensions consistently follow the pattern:

```csharp
public static IServiceCollection AddOmexServices(this IServiceCollection collection)
```

See `src/Hosting/ServiceCollectionExtensions.cs` for the canonical example.

#### Accessor Pattern

Use `IAccessor<T>` for late-bound dependencies not available during DI container build:

```csharp
public interface IAccessor<out TValue> where TValue : class
{
    TValue? Value { get; }
    TValue GetValueOrThrow();
    void OnFirstSet(Action<TValue> function);
}
```

#### Health Check Composition

Health checks use composable decorators instead of inheritance:

- Wrap base checks with `ObservableHealthCheck` for telemetry
- Use `StartupHealthCheck` for one-time validation caching
- Compose via `HealthCheckComposablesExtensions` factory methods

#### Environment Configuration

Use `OmexEnvironments` constants instead of magic strings:

- `Development`, `Int` (CI/CD), `EDog` (pre-prod), `Production`

## Build & Development Patterns

### Project Structure

- All projects use `Microsoft.Omex.Extensions.*` naming
- Source in `src/`, tests in `tests/` with matching directory structure
- Central package management via `Directory.Packages.props`
- Multi-targeting: .NET 9.0 + netstandard2.0 for libraries

### Build Configuration

- `Directory.Build.props` enforces: nullable reference types, treat warnings as errors, strong naming
- Tests use MSTest framework with Moq
- Service Fabric SDK required for SF-related projects

### Validation & Tagging

Use `Validation.ThrowIfNullOrWhiteSpace()` for parameter validation.
Create EventIds with `Tag.Create()` using CallerFilePath/CallerLineNumber for automatic tagging.

## Testing Conventions

- Test projects mirror source structure: `*.UnitTests` suffix
- MSTest with `[TestClass]` and `[TestMethod]` attributes
- Use `[DataRow]` for parameterized tests
- Follow pattern: `MethodName_WhenCondition_ExpectedResult`

## Dependencies & Service Fabric

- Heavy integration with Microsoft.Extensions.\* ecosystem
- Service Fabric components require specific SDK installation
- HttpClient usage through `IHttpClientFactory` pattern
- Activity/telemetry integration throughout all components

## Key Commands

```bash
dotnet restore
dotnet build --configuration Release
dotnet test --no-build --configuration Release
dotnet pack --no-build --configuration Release
```

Service Fabric development requires Windows with SF SDK installed.
