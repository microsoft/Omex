## Health Check ASP.NET Core helpers and extensions

This project introduces features specifically dependent from the ASP.NET Core WebAPI Framework. This will not introduce other composable classes, but it will leverage them to provide a new factory method to create a **Liveness Health Check**, an health check that will only verify that the endpoint is reachable without letting it execute its logic, short-circuiting it using an implementatin of the `ActionFilterAttribute`.

#### Liveness Health Check

The liveness health check is a special type of health check that limits its probing inspection to the reachability of the endpoint. This means that it will not execute the endpoint logic, that will be short-circuited by the `LivenessCheckActionFilterAttribute`. The mechanic of this interaction is the following:

- The Health Check implementor will call the `CreateLivenessHttpHealthCheck` method in the [`HealthCheckComposablesExtensions`](./HealthCheckComposablesExtensions.cs) class. This factory method will create a default Http Health Check, but it will tweak the Activity by injecting the `activityMarker` function in the constructor, adding a key to the `Activity` baggage.
- The Activity will be send to the endpoint.
- The endpoint will have to be marked with the [`LivenessCheckActionFilterAttribute`](./LivenessCheckActionFilterAttribute.cs). This filter will check whether there's an `Activity` injected in the request, and whether the `Activity` has the required key in its baggage: if that is the case, the endpoint execution will be short-circuited, returning an HTTP 200 status code, otherwise the method will be executed normally.
