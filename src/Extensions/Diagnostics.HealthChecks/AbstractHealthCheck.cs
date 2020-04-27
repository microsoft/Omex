// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Omex.Extensions.Abstractions.Activities;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	/// <summary>
	/// Base health check that extracts logic of exception handling and wraps it into timed scope
	/// </summary>
	public abstract class AbstractHealthCheck : IHealthCheck
	{
		private static readonly TimedScopeDefinition s_scopeDefinition = new TimedScopeDefinition("HealthCheckScope");

		private readonly ITimedScopeProvider m_scopeProvider;

		/// <summary>
		/// Base constructor with scope provider that would be removed after .NET 5 move
		/// </summary>
		protected AbstractHealthCheck(ITimedScopeProvider scopeProvider) =>
			m_scopeProvider = scopeProvider;

		/// <inheritdoc />
		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken token = default)
		{
			using TimedScope scope = m_scopeProvider.CreateAndStart(s_scopeDefinition).MarkAsHealthCheck();

			try
			{
				HealthCheckResult result = await CheckHealthInternalAsync(context, token).ConfigureAwait(false);

				scope.SetResult(TimedScopeResult.Success);

				return result;
			}
			catch (Exception exception)
			{
				return HealthCheckResult.Unhealthy("HealthCheck failed", exception);
			}
		}

		/// <summary>
		/// Health check logic without error handling and timed scope wrapping
		/// </summary>
		protected abstract Task<HealthCheckResult> CheckHealthInternalAsync(HealthCheckContext context, CancellationToken token);
	}
}
