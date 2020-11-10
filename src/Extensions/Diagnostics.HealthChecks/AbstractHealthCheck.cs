// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Abstractions.Activities;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	/// <summary>
	/// Base health check that extracts logic of exception handling and wraps it into timed scope
	/// </summary>
	public abstract class AbstractHealthCheck<TParameters> : IHealthCheck
		where TParameters : HealthCheckParameters
	{
		private static readonly TimedScopeDefinition s_scopeDefinition = new TimedScopeDefinition("HealthCheckScope");

		private readonly ITimedScopeProvider m_scopeProvider;

		/// <summary>
		/// Logger property
		/// </summary>
		protected ILogger Logger { get; }

		/// <summary>
		/// Logger property
		/// </summary>
		protected internal TParameters Parameters { get; } // internal only to be used for unit tests

		/// <summary>
		/// Base constructor with scope provider that would be removed after .NET 5 move
		/// </summary>
		protected AbstractHealthCheck(TParameters parameters, ILogger logger, ITimedScopeProvider scopeProvider)
		{
			Parameters = parameters;
			Logger = logger;
			m_scopeProvider = scopeProvider;
		}

		/// <inheritdoc />
		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken token = default)
		{
			using TimedScope scope = m_scopeProvider.CreateAndStart(s_scopeDefinition).MarkAsHealthCheck();

			try
			{
				HealthCheckResult result = await CheckHealthInternalAsync(context, token).ConfigureAwait(false);
				result = EnforceFailureStatus(context.Registration.FailureStatus, result);

				scope.SetResult(TimedScopeResult.Success);

				return result;
			}
			catch (Exception exception)
			{
				Logger.LogError(Tag.Create(), exception, "'{0}' check failed with exception", context.Registration.Name);
				return new HealthCheckResult(context.Registration.FailureStatus, "HealthCheck failed", exception, Parameters.ReportData);
			}
		}

		/// <summary>
		/// Health check logic without error handling and timed scope wrapping
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
