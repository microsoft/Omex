// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.Activities;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	/// <summary>
	/// Base health check that extracts logic of exception handling and wraps it into activity
	/// </summary>
	[Obsolete("The usage of this class is deprecated, please use composable classes in Microsoft.Omex.Extensions.Diagnostics.HealthChecks.Composables namespace to build health checks.")]
	public abstract class AbstractHealthCheck<TParameters> : IHealthCheck
		where TParameters : HealthCheckParameters
	{
		private readonly ActivitySource m_activitySource;

		/// <summary>
		/// Logger property
		/// </summary>
		protected ILogger Logger { get; }

		/// <summary>
		/// Logger property
		/// </summary>
		protected internal TParameters Parameters { get; } // internal only to be used for unit tests

		/// <summary>
		/// Base constructor
		/// </summary>
		protected AbstractHealthCheck(TParameters parameters, ILogger logger, ActivitySource activitySource)
		{
			Parameters = parameters;
			Logger = logger;
			m_activitySource = activitySource;
		}

		/// <inheritdoc />
		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken token = default)
		{
			string activityName = string.IsNullOrWhiteSpace(context.Registration.Name) ? "HealthCheckActivity" : context.Registration.Name;
			using Activity? activity = m_activitySource.StartActivity(activityName)
				?.MarkAsSystemError()
				.MarkAsHealthCheck();

			try
			{
				HealthCheckResult result = await CheckHealthInternalAsync(context, token).ConfigureAwait(false);
				result = EnforceFailureStatus(context.Registration.FailureStatus, result);

				activity?.MarkAsSuccess();

				return result;
			}
			catch (Exception exception)
			{
				Logger.LogError(Tag.Create(), exception, "'{0}' check failed with exception", context.Registration.Name);
				return new HealthCheckResult(context.Registration.FailureStatus, "HealthCheck failed", exception, Parameters.ReportData);
			}
		}

		/// <summary>
		/// Health check logic without error handling and activity wrapping
		/// </summary>
		protected abstract Task<HealthCheckResult> CheckHealthInternalAsync(HealthCheckContext context, CancellationToken token);

		private static HealthCheckResult EnforceFailureStatus(HealthStatus failureStatus, HealthCheckResult result)
		{
			if (result.Status == HealthStatus.Healthy || result.Status == failureStatus)
			{
				return result;
			}

			return new HealthCheckResult(failureStatus, result.Description, result.Exception, result.Data);
		}
	}
}
