// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.Composables;

/// <summary>
/// Performs an HTTP endpoint call and then checks the result of the call.
/// </summary>
public sealed class HttpEndpointHealthCheck : IHealthCheck
{
	private const string ActivityName = nameof(HttpEndpointHealthCheck);

	private readonly IHttpClientFactory m_httpClientFactory;
	private readonly Func<HttpRequestMessage> m_requestBuilder;
	private readonly Func<HealthCheckContext, HttpResponseMessage, CancellationToken, Task<HealthCheckResult>> m_healthCheckResponseChecker;
	private readonly Action<Activity?>? m_activityMarker;
	private readonly ActivitySource m_activitySource;
	private readonly string? m_httpClientName;

	/// <summary>
	/// Constructor.
	/// </summary>
	/// <param name="httpClientFactory">The HTTP Client factory.</param>
	/// <param name="requestBuilder">Function that returns the request.</param>
	/// <param name="healthCheckResponseChecker">Function that checks the HTTP response from the health check targeted endpoing.</param>
	/// <param name="activitySource">The activity source for observability.</param>
	/// <param name="httpClientName">The name of the HTTP Client, if a particular client with that registration name has been defined.</param>
	/// <param name="activityMarker">Delegate that marks the activity with the desired bagging items.</param>
	public HttpEndpointHealthCheck(
		IHttpClientFactory httpClientFactory,
		Func<HttpRequestMessage> requestBuilder,
		Func<HealthCheckContext, HttpResponseMessage, CancellationToken, Task<HealthCheckResult>> healthCheckResponseChecker,
		ActivitySource activitySource,
		string? httpClientName = "",
		Action<Activity?>? activityMarker = null)
	{
		m_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
		m_requestBuilder = requestBuilder ?? throw new ArgumentNullException(nameof(requestBuilder));
		m_healthCheckResponseChecker = healthCheckResponseChecker ?? throw new ArgumentNullException(nameof(healthCheckResponseChecker));
		m_activitySource = activitySource ?? throw new ArgumentNullException(nameof(activitySource));
		m_httpClientName = httpClientName;
		m_activityMarker = activityMarker;
	}

	/// <inheritdoc />
	public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
	{
		using Activity? activity = m_activitySource.StartActivity($"{context.Registration.Name}_{ActivityName}");
		m_activityMarker?.Invoke(activity);

		HttpClient httpClient = m_httpClientName == null || string.IsNullOrWhiteSpace(m_httpClientName)
			? m_httpClientFactory.CreateClient()
			: m_httpClientFactory.CreateClient(m_httpClientName);

		HttpRequestMessage request = m_requestBuilder();

		try
		{
			HttpResponseMessage? response = await httpClient.SendAsync(request, cancellationToken);

			HealthStatus healthStatus = HealthStatus.Unhealthy;
			string description = string.Empty;

			if (response != null)
			{
				return await m_healthCheckResponseChecker(context, response, cancellationToken);
			}

			HealthCheckResult result = new(healthStatus, description);

			return result;
		}
		catch (HttpRequestException ex)
		{
			return new HealthCheckResult(
				context.Registration.FailureStatus,
#if NET5_0_OR_GREATER
				description: $"HTTP Health Check failed with Status Code '{ex.StatusCode}'",
#else
				description: $"HTTP Health Check failed",
#endif
				exception: ex);
		}
		catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
		{
			return new HealthCheckResult(
				context.Registration.FailureStatus,
				description: $"HTTP Health Check failed for timeout.",
				exception: ex);
		}
	}
}
