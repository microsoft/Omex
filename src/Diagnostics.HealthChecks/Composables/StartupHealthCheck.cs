// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.Composables;

/// <summary>
/// Wraps the execution of a health check. The execution will be retried until the health check will succeed
/// of the service will be killed. If the call succeeds, the response will be saved in a memory cache, and
/// the health check execution will cease.
/// </summary>
public sealed class StartupHealthCheck : IHealthCheck
{
	private readonly IMemoryCache m_memoryCache;
	private readonly IHealthCheck m_wrappedHealthCheck;

	/// <summary>
	/// Constructor.
	/// </summary>
	/// <param name="healthCheck">The wrapped health check, whose check method will be called.</param>
	/// <param name="memoryCache">
	/// The memory cache instance to register the health check result.
	/// </param>
	public StartupHealthCheck(
		IHealthCheck healthCheck,
		IMemoryCache memoryCache)
	{
		m_wrappedHealthCheck = healthCheck ?? throw new ArgumentNullException(nameof(healthCheck));
		m_memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
	}

	/// <inheritdoc />
	public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
	{
		string healthCheckRegistrationName = context.Registration.Name;

		if (TryGetHealthyStatusCached(healthCheckRegistrationName, out HealthCheckResult cachedResult))
		{
			return cachedResult;
		}

		// The execution will not be wrapped in an exception, because if the health check throws an exception,
		// the health check has not succeeded, and it will not be saved.
		HealthCheckResult result = await m_wrappedHealthCheck.CheckHealthAsync(context, cancellationToken);

		CacheHealthyStatus(healthCheckRegistrationName, result);

		return result;
	}

	/// <summary>
	/// Uses the in-memory cache to check whether the health check has been already performed or not.
	/// </summary>
	/// <param name="healthCheckRegistrationName">The health check registration name.</param>
	/// <param name="result">The cached result.</param>
	/// <returns><c>True</c> if the health check has been performed already, <c>False</c> otherwise.</returns>
	private bool TryGetHealthyStatusCached(string healthCheckRegistrationName, out HealthCheckResult result) =>
		m_memoryCache.TryGetValue(healthCheckRegistrationName, out result) &&
		result.Status == HealthStatus.Healthy;

	/// <summary>
	/// Uses the in-memory cache to set a value indicating that the check has been already performed.
	/// </summary>
	/// <param name="healthCheckRegistrationName">The health check registration name.</param>
	/// <param name="result">The result of the health check.</param>
	/// <returns><c>True</c> if the health check has been performed already, <c>False</c> otherwise.</returns>
	private void CacheHealthyStatus(string healthCheckRegistrationName, HealthCheckResult result)
	{
		if (result.Status == HealthStatus.Healthy)
		{
			m_memoryCache.Set(healthCheckRegistrationName, result);
		}
	}
}
