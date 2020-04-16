# Omex extension packages
## Overview

Packages created to provide Omex implementation of Extension interfaces that would be part of .NET 5 and currently distributed as nuget packges https://github.com/aspnet/Announcements/issues/411

Packages use concept called Generic Host that is a set of interfaces for abstracting service run form infrastructure

* `Microsoft.Omex.Extensions.Abstractions` - common primitives and utility classes, should be used for library creation
* `Microsoft.Omex.Extensions.Logging` - implementation of `ILogger` interface that sends messages into our log aggregation system`
* `Microsoft.Omex.Extensions.TimedScopes` - library for sending telemetry from `Activity` and `TimedScopes` to our log aggregation system
* `Microsoft.Omex.Extensions.Compatibility` - provides access to some of the depricated static classes like `UlsLogger`, `Code.Validate`, `TimedScopes.Create`
* `Microsoft.Omex.Extensions.Hosting` - package for cretion simple application (ex. Console app) based on Generic Host
* `Microsoft.Omex.Extensions.Hosting.Services` - package for creation of Service Fabric services base on Generic Host
* `Microsoft.Omex.Extensions.Hosting.Services.Web` - package for defining web listeners (based on Kestrel) in Service Fabric services
* `Microsoft.Omex.Extensions.Hosting.Services.Remoting` - package for defining remoting listeners in Service Fabric services
* `Microsoft.Omex.Extensions.Services.Remoting` - package for creating remoting listeners in Service Fabric

## Goals
* Isolation from Omex infrastructure
* Isolation from Service Fabric
* Using new Dependency Injection with resolution validation
* Using null reference types
* Replace `TimedScope` with `Activity`

## Changed concepts
### Logger
Static `UlsLogger` replaced with `ILogger<TCategoryName>` that is taken from DI, genric type `TCategoryName` is usually the type where you are ingecting logger and name of the type would be used as logger category.
Log levels are changed as following:
```
Error => LogLevel.Error
Info => LogLevel.Information
Spam => LogLevel.Trace
Verbose => LogLevel.Debug
Warning => LogLevel.Warning
```
Level description here: https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel?view=dotnet-plat-ext-3.1
More about `ILogger`: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-3.1

### Tagger
Currently we have two ways of creating log tags (now called `EventId`):
1. If you are using `Tag.ReserveTag(0)` tag would be replaced by the tool called GitTagger and commited to source control
2. If you are using `Tag.Create()` it would use compiller attributes like `CallerFileName` and `CallerLineNumber` to create tag based on the position in code where log method is called, this approach in still in development and not officially approved yet

### TimedScopes
`TimedScopes` is this implementation is just a wrapper on top of `Activity` primitive and after .NET 5 release would be completely replaced with `Activity`.
For now to create `TimedScope` you need `ITimedScopeProvider` and `TimedScopeDefinition`.
*  `ITimedScopeProvider` - could be obtained from DI for now, after .NET 5 relese there will be no need in using DI (Currently it's required only for LogReplay logic).
* `TimedScopeDefinition` now created manually (before it was generated from xml file by TimedScopeGen tool) and usually located stored as a static properties of static class in a separete project (to simplify their export to diagnostic).

After creation of `TimedScope`/`Activity` you can use extension methods like `SetResult`, `SetSubType`, `SetMethadata` to enritch `Activity` adition with information.

If code is executed inside `Activity` you can always access this it using `Activity.Current` otherwise this it's value will be `null` (currently filed type marked as not nullable but it would be updated).

```csharp
Activity.Current //would be null here
ITimedScopeProvider provider = ... //Get from DI
TimedScopeDefinition definition = new TimedScopeDefinition("Some Name"); // usually located taken from a property in a special static class
using (TimedScope scope = provider.CreateAndStart(definition))
{
  scope
    .SetResult(TimedScopeResult.Success)
    .SetMethadata("Important info 1");

  Activity.Current //would have corresponding object inside

  Activity.Current
    .SetResult(TimedScopeResult.Success)
    .SetMethadata("Important info 2");
}
```

More about `Activity`: https://github.com/dotnet/runtime/blob/master/src/libraries/System.Diagnostics.DiagnosticSource/src/ActivityUserGuide.md
New design of `Activity`: https://github.com/dotnet/designs/pull/98

### Dependency Injection
There are following diffirences comparing with dependency injection that we used before:
1. Separate phase of types registration, then creation of container that is able to resolve types. Such approach significantly simplify usage of the container since we could never get uninitialized or partially initialized object.
2. During creation of DI container it will validate that it's able to resolve all registered types and it will throw exceptions if any type not resolvable (not working for open generics and some other cases).
3. If during type resolution DI unable to resolve constructor parameter it will throw exception with explanation, if you want create type even if parameter now available you shoud provide default value for it for example `MyType(NotRegistredType obj = null)`.

//TODO: add more details

Problems:
1. SF types registration
2. Web Listeners DI

### Input validation
By default if you are refencing `Microsoft.Omex.Extensions.Abstractions` package, it will enable nullable reference types on you project, but it could be disabled by adding `<Nullable>disable</Nullable>` property into project.

Nullable reference types for now not garrante that you won't get not nullable input from user or even from standard types (they are in a process of updating), but they are adding build type validation for not those types.

TODO: add more details

### Settings
Settings a changed to using `IOptions` and special config providers to get settings from SOS or SF settings
Currently they are not opensourced so could be found in internal repo and wiki
Additional information about IOptions: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-3.1

### Service definition
```csharp
public async Task Main(string[] args)
{
  await Host
    .CreateDefaultBuilder(args)
    .AddOmexCompatabilityServices()  // Microsoft.Omex.Extensions.Compatability, needed only if you want to use ULSLogger or Code.Validate
    .BuildStatelessService(  // Microsoft.Omex.Extensions.Hosting.Services
      "MyAwesomeService",
      builder =>
      {
        builder
          .AddServiceAction(RunAsyncAction1)
          .AddServiceAction(RunAsyncAction2)
          .AddServiceListener("MyListener1", Listener1)
          .AddServiceListener("MyListener2", Listener2)
          .AddKestrelListener<Startup>(  // Microsoft.Omex.Extensions.Hosting.Services.Web
            "MyWebListener",
            ServiceFabricIntegrationOptions.UseReverseProxyIntegration);
      })
    .RunAsync();
}
```
### Compatability
#### Web listener definition
#### Remoting listener definition
#### Remoting clients