## Omex Health Checks

This project and the [`Diagnostics.HealthChecks.AspNetCore`](../Diagnostics.HealthChecks.AspNetCore/Readme.md) offers a range of composable classes, all of which implements the .NET `IHealthCheck` interface, to help the developer build their own Health Checks by reusing different behaviours with the help of extension methods.

This approach enables the ability to create new composable objects to enrich the single Health Check with different behaviour, or substitute an existing one in the composition chain, offering more flexibility to the

In the following, each different composable classes provided by the package will be listed, followed by utilisation examples.

The last section of this guide will describe the old approach and its porting to the new implementation.

### Composable Health Checks classes

#### [`ObservableHealthCheck`](./Composables/ObservableHealthCheck.cs)

This Health Check provides a default implementation for exception handling for the built health check. This implementation also offers Activity creation and marking as health check, enabling the implementation of custom behaviours in the targeted controller endpoint.

"Marking" an activity means that the `Activity`'s baggage will be populated with a key and a corresponding value; WebAPI framework automatically sends the `Activity` information along with the request from an `HttpClient` call, allowing the receiver to fetch the `Activity` baggage values and acting accordingly. This feature will be leveraged when instantiating a [Liveness health check](../Diagnostics.HealthChecks.AspNetCore/HealthCheckComposablesExtensions.cs#liveness-health-check).
The `ObservableHealthCheck` will mark the `Activity` using the `MarkAsHealthCheck` method in the [`ActivityExtensions`](../Abstractions/Activities/ActivityExtensions.cs) class.

#### [`StartupHealthCheck`](./Composables/StartupHealthCheck.cs)

This implementation offers the capability of executing the health check until it succeeds, then it will re-use the last cached successful result without repeating the Health Check execution. This behaviour is useful when an health check must be executed only at startup, to check for instance for configuration errors, reporting them back to Service Fabric engine while deploying to mark the new service as faulty, enabling the orchestrator to rollback the deployment.

The developer must keep in mind that an health check that includes the startup health check in its implementation will keep executing if the health check result results in a warning or in an error state; this can happen when the service the health check it's implemented into is created by Service Fabric in response to an automatic scale-up.

The developer must also keep in mind that to use this composable Health Check they will have to provide an implementation of the [`IMemoryCache`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.memory.imemorycache?view=dotnet-plat-ext-7.0) interface through Dependency Injection.

#### [`HttpEndpointHealthCheck`](./Composables/HttpEndpointHealthCheck.cs)

This implementation covers the most common usage of health checks. It will send a request to a controller endpoint in the context of the Service Fabric service (i.e. it will perform the call to `localhost`), it will handle the most common exceptions linked to HTTP calls using `HttpClient`, and it will eventually check the response for it to respond to the requirements.

This health check offers extensibility through the definition of different functions:

- `requestBuilder`: this function will have to return the request message that will be used by the `HttpClient` to perform the request.
- `healthCheckResponseChecker`: this function will receive in input all the information about the HTTP call response; its responsibility will be to report the Health Check Status as a response, and it will be used to determine the result of the Health Check.

In an effort to keep the Health Check as general as possible, the constructor will need an instance of the following:

- `IHttpClientFactory`: it will be the responsibility of the implementor to provide a way to create the `HttpClient` instance. This will be addressed in the [Implementation examples](#implementation-examples).
- `ActivitySource`: this Health Check will create an `Activity` instance. If a wrapper implementation in the composition classes chain created one already, it should be linked to the newly created one inside this health check.

The `HttpEndpointHealthCheck` offers the possibility to "mark" the created `Activity` with custom markers by defining the `activityMarker` function in the constructor.

### Implementation examples

Implementing an Health Check using the composable classes provided by default can be done following two patterns:

- **Creating the classes manually**: this approach consists on creating the classes manually inside the constructor of the Health Check that will be registered for the application. The resulting Health Check will have to declare the join set of dependencies needed by all the default classes used, along with its own.
- **Leveraging the extension methods provided in the project**: this project provides a set of extension methods located in the [`HealthCheckComposablesExtensions`](./Composables/HealthCheckComposablesExtensions.cs) class that will complement the functionality offered by the default composable classes with default composition building for different use cases.

In each cases, having that the Health Check will have to be registered with the DI, the implementor will have to create a class implementing the `IHealthCheck` interface directly, composing the different health checks in the constructor and storing the reference in a private field in the class.

#### Manual composition

In this case, the final Health Check class will have to instantiate the Health Checks composable classes manually in the constructor.

Let's say for instance that the Health Check will have to constantly query an endpoint in the service, the class constructor can be implemented as:

```csharp
public class CustomHealthCheck : IHealthCheck
{
    private IHealthCheck m_composed;

    public CustomHealthCheck(IHttpClientFactory factory, ...)
    {
        // In the constructor, the health check instance will be built manually, passing along all the necessary dependencies.
        IHealthCheck httpHealthCheck = new HttpEndpointHealthCheck(...);
        m_composed = new ObservableHealthCheck(httpHealthCheck, ...);
    }

    // The method simply forward the health check implementation to the composed instance.
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
        m_composed.CheckHealthAsync(context, cancellationToken);
}
```

This approach offers more flexibility and customisation, but it introduces more complexity and coupling with this project.

#### Composing using extension methods

The [`HealthCheckComposablesExtensions`](./Composables/HealthCheckComposablesExtensions.cs) class offers a number of health check factories that will create composed classes ready to be used for a number of different cases. If the developer wants to implement a simple HTTP Health Check like in the [previous example](#manual-composition), for instance, they can leverage the `CreateHttpHealthCheck` method:

```csharp
public class CustomHealthCheck : IHealthCheck
{
    private IHealthCheck m_composed;

    public CustomHealthCheck(IHttpClientFactory factory, ...)
    {
        // Instead of manually creating the classes, the constructor calls the static factory method that will create those classes for it.
        m_composed = HealthCheckComposableExtensions.CreateHttpHealthCheck(
            RequestBuilder,
            HealthCheckComposableExtensions.CheckResponseStatusCodeAsync,
            ...);
    }

    // The method simply forward the health check implementation to the composed instance.
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
        m_composed.CheckHealthAsync(context, cancellationToken);
}
```

This approach is less flexible, but it provides more readability out of the box by representing exactly what the health check should do through the static factory method name.

More factory methods will be presented in the [Diagnostics.HealthChecks.AspNetCore](../Diagnostics.HealthChecks.AspNetCore/Readme.md) project.