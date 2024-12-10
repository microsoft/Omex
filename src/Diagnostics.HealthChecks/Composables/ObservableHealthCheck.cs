// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.Activities;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.Composables;

/// <summary>
/// The observable health check. This Health Check should wrap every other IHealthCheck instance.
/// The responsibility of this health check decorator is to configure telemetry for the health check activity,
/// and handle the exception thrown by wrapped components, to return a health check result consistent
/// with the Health Check parameters.
/// </summary>
public sealed class ObservableHealthCheck : IHealthCheck
{
	private readonly HealthCheckParameters m_parameters;
	private readonly IHealthCheck m_wrappedHealthCheck;
	private readonly ActivitySource m_activitySource;
	private readonly ILogger m_logger;

	/// <summary>
	/// Constructor.
	/// </summary>
	/// <param name="parameters">
	/// The health check parameters passed at DI configuration time.
	/// They can be used to specify escalation configuration (e.g. the IcM ticket severity, owning team),
	/// </param>
	/// <param name="healthCheck">The health check instance to wrap.</param>
	/// <param name="activitySource">The activity source.</param>
	/// <param name="logger">The logger.</param>
	public ObservableHealthCheck(
		HealthCheckParameters parameters,
		IHealthCheck healthCheck,
		ActivitySource activitySource,
		ILogger logger)
	{
		m_parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
		m_wrappedHealthCheck = healthCheck ?? throw new ArgumentNullException(nameof(healthCheck));
		m_activitySource = activitySource ?? throw new ArgumentNullException(nameof(activitySource));
		m_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <inheritdoc />
	public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
	{
		using Activity? activity = m_activitySource.StartActivity(context.Registration.Name)
			?.MarkAsHealthCheck()
			?.MarkAsSystemError();

		try
		{
			HealthCheckResult result = await m_wrappedHealthCheck.CheckHealthAsync(context, cancellationToken);

			try
			{
				activity?.SetTag("HealthCheckResult", result.Status.ToString());
			}
			catch (Exception ex) {
				m_logger.LogError(Tag.Create(), ex, "'{registrationName}' health check tag addition failed", context.Registration.Name);
			}

			activity?.MarkAsSuccess();

			// The health status for the health check result: if the status is healthy, it will be returned as it is,
			// if not then registration failure status will be sent in its place.
			HealthStatus healthCheckStatus = result.Status == HealthStatus.Healthy
				? result.Status
				: context.Registration.FailureStatus;

			return new HealthCheckResult(
				healthCheckStatus,
				result.Description,
				data: m_parameters.ReportData,
				exception: result.Exception);
		}
		catch (Exception ex)
		{
			m_logger.LogError(Tag.Create(), ex, "'{0}' check failed with exception", context.Registration.Name);
			return new HealthCheckResult(context.Registration.FailureStatus, "HealthCheck failed", ex, data: m_parameters.ReportData);
		}
	}
}
