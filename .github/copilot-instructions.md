# Microsoft Omex Extensions - Copilot Instructions

## Project Overview

This is a collection of shared .NET extensions and utilities for building scalable, distributed systems at Microsoft. The codebase follows a modular architecture with multiple NuGet packages, each serving specific purposes in service orchestration, telemetry, health monitoring, Service Fabric integration, and feature management.

## Architecture & Module Structure

### Core Modules

- **Abstractions**: Base interfaces and utilities (`IAccessor<T>`, validation, tagging)
- **Activities**: OpenTelemetry/activity management and metrics collection with W3C trace context
- **Hosting**: Service hosting utilities with environment-specific configurations (`OmexEnvironments`)
- **Logging**: Centralized logging infrastructure with scrubbing capabilities
- **Diagnostics.HealthChecks**: Composable health check framework using decorator pattern
- **FeatureManagement**: Advanced feature flags with A/B testing, experimental features, and query overrides
- **ServiceFabricGuest.Abstractions**: Service Fabric client wrappers and abstractions
- **Services.Remoting**: Service Fabric remoting extensions with activity propagation
- **Testing.Helpers**: Shared testing utilities including `NullableAssert`

### Key Patterns

#### Service Registration Pattern

All extensions follow this consistent pattern:

```csharp
public static IServiceCollection AddOmexServices(this IServiceCollection collection)
{
    // Always use TryAdd* methods for idempotency
    // Chain multiple registrations fluently
    return collection.AddOmexActivitySource();
}
```

See `src/Hosting/ServiceCollectionExtensions.cs` for the canonical example.

#### Accessor Pattern

Use `IAccessor<T>` for late-bound dependencies not available during DI container build (especially Service Fabric contexts):

```csharp
// Always register with TryAddAccessor<T>() helper
collection.TryAddAccessor<StatefulServiceContext, ServiceContext>();

// Implementation handles weak references for callback cleanup
public void OnFirstSet(Action<TValue> action)
{
    if (m_value != null) action(m_value);
    else m_actions.AddLast(new WeakReference<Action<TValue>>(action));
}
```

See `src/Abstractions/Accessors/Accessor.cs` and `src/Hosting.Services/HostBuilderExtensions.cs`

#### Health Check Composition

Health checks use decorator pattern instead of inheritance:

```csharp
// Wrap base checks with observability
services.AddHealthCheck("MyCheck")
    .AddObservability()  // Telemetry wrapper
    .AddStartupCaching() // One-time validation
    .Build();
```

#### Feature Management Integration

The system supports layered feature resolution with experimental overrides:

```csharp
// Always inject IFeatureGatesConsolidator (higher level)
private readonly IFeatureGatesConsolidator _featureGates;

// Resolution order: Query params → Experiments → Static config
var features = await _featureGates.GetFeatureGatesAsync(filters);
```

#### Environment Configuration

Use `OmexEnvironments` constants instead of magic strings:

```csharp
// From src/Hosting/OmexEnvironments.cs
OmexEnvironments.Development  // "Development"
OmexEnvironments.Int         // "Int" (CI/CD)
OmexEnvironments.EDog        // "EDog" (pre-prod)
OmexEnvironments.Production  // "Production"
```

#### Validation & Tagging

Always use project-specific validation and automatic tagging:

```csharp
// Parameter validation (throws with descriptive messages)
string validated = Validation.ThrowIfNullOrWhiteSpace(value, nameof(value));

// Automatic EventId generation using file path and line number
EventId eventId = Tag.Create(); // Uses [CallerFilePath] and [CallerLineNumber]
logger.LogInformation(eventId, "Message");
```

## Build & Development Patterns

### Project Structure

- All projects use `Microsoft.Omex.Extensions.*` naming convention
- Source in `src/`, tests in `tests/` with matching directory structure
- Central package management via `Directory.Packages.props` with version conditions
- Multi-targeting: .NET 10.0 + netstandard2.0 for broad compatibility
- Strong naming with `OmexOpenSource.snk`

### Build Configuration

Critical settings in `Directory.Build.props`:

- Nullable reference types enabled (`<Nullable>enable</Nullable>`)
- Treat warnings as errors (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`)
- Automatic `InternalsVisibleTo` for unit test assemblies and Moq
- Source linking for debugging with path mapping for build location independence

### Testing Conventions

- Test projects mirror source structure with `*.UnitTests` suffix
- MSTest framework with `[TestClass]` and `[TestMethod]` attributes
- Use `[DataRow]` for parameterized tests extensively
- Test naming: `MethodName_WhenCondition_ExpectedResult`
- Mock with Moq (pinned to version 4.20.72)
- Shared testing utilities in `Testing.Helpers` project

Example test structure:

```csharp
[TestMethod]
[DataRow(typeof(IAccessor<ServiceContext>), typeof(Accessor<StatefulServiceContext>))]
public void BuildService_RegistersTypes(Type interfaceType, Type implementationType)
```

## Dependencies & Integration Points

### Core Microsoft.Extensions Integration

- Heavy use of Options pattern with validation
- HttpClient factory pattern for all HTTP operations
- Hosted services for background initialization
- Configuration binding with `appsettings.json`

### Activity/Telemetry Integration

- W3C trace context propagation across service boundaries
- Custom baggage and tag dimensions through `ICustomBaggageDimensions`
- Activity metrics with histogram reporting
- Service Fabric remoting headers: `omex-traceparent`, `omex-tracestate`

### Service Fabric Specifics

- Requires Service Fabric SDK for development
- Accessor pattern critical for context availability during service lifecycle
- State manager integration for stateful services
- Partition and replica role change handling

## Key Commands

```powershell
# Basic build workflow
dotnet restore
dotnet build --configuration Release
dotnet test --no-build --configuration Release
dotnet pack --no-build --configuration Release

# Service Fabric development (Windows required)
# Ensure Service Fabric SDK installed for SF projects
```

## Recent Architecture Additions

### Feature Management System

- Layered resolution: Query overrides → Experiments → Static config
- Support for A/B testing with customer targeting
- HTTP context integration for web scenarios
- Frontend feature handling with automatic header processing

### Modern Telemetry

- Migration from EventSource to OpenTelemetry metrics
- Histogram-based activity measurement with tenant/environment tagging
- Activity observer pattern for consistent logging format
