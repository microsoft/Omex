## Health Check ASP.NET Core helpers and extensions

This project introduces features specifically designed for the ASP.NET Core WebAPI Framework. This will not introduce other composable classes, but it will leverage them to provide a new factory method to create a **Liveness Health Check**, an health check that will only verify that the endpoint is reachable without letting it execute its logic, short-circuiting it using an implementatin of the `ActionFilterAttribute`.

#### Liveness Health Check

The liveness health check is a special type of health check that limits its probing inspection to the reachability of the endpoint. This means that it will not execute the endpoint logic, that will be short-circuited by the `LivenessCheckActionFilterAttribute`. The mechanic of this interaction is the following:

- The Health Check implementor will call the `CreateLivenessHttpHealthCheck` method in the [`HealthCheckComposablesExtensions`](./HealthCheckComposablesExtensions.cs) class. This factory method will create a default Http Health Check, but it will tweak the Activity by injecting the `activityMarker` function in the constructor, adding a key to the `Activity` baggage.
- The Activity will be sent to the endpoint.
- The endpoint will have to be marked with the [`LivenessCheckActionFilterAttribute`](./LivenessCheckActionFilterAttribute.cs). This filter will check whether there's an `Activity` injected in the request, and whether the `Activity` has the required key in its baggage: if that is the case, the endpoint execution will be short-circuited, returning an HTTP 200 status code, otherwise the method will be executed normally.

### Extension methods

The project offers extension methods to easily register liveness health checks on endpoints that can be specified with one line of configuration. The extension methods are located in the `HealthCheckComposableExtensions`, and they will extend the functionality on the `IHealthCheckBuilder` class available in the DI.

#### AddEndpointHttpHealthCheck

There are two overloads of the `HealthCheckComposableExtensions.AddEndpointHttpHealthCheck`. The first one allows the developer to specify directly the custom `HttpClientParameters` class called `EndpointLivenessHealthCheckParameters`: this class contains all the required information on how to build the HTTP Request to query the endpoint to check.

The second overload has been implemented to offer a similar functionality to the one available using the legacy health checks in the `HealthCheckComposablesExtensions` class: in this overload, all the information previously included in the `EndpointLivenessHealthCheckParameters` class can be defined manually.

For instance, in the following code it is shown how to define an HTTP Health Check on the `/healthz` endpoint using the legacy extension method:

```csharp
services.AddServiceFabricHealthChecks().AddHttpEndpointCheck("healthz", endpointName, "/healthz");
```

Using the new extension method, the same functionality can be achieved with the following call:

```csharp
services.AddServiceFabricHealthChecks().AddEndpointHttpHealthCheck("healthz", endpointName, "/healthz", "httpClientLogicalName");
```

The only difference is the specification of the `HttpClient` logical name, as the definition of it will now be a responsibility of the DI configuration. 
