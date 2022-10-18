// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Omex.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Omex.Extensions.DependencyInjection;

/// <summary>
/// A builder used to register health checks.
/// </summary>
public interface IOmexHealthChecksBuilder : IHealthChecksBuilder
{
	/// <summary>
	/// Adds a <see cref="HealthCheckRegistration"/> for a health check.
	/// </summary>
	/// <param name="registration">The <see cref="HealthCheckRegistration"/>.</param>
	/// <param name="parameters">The <see cref="HealthCheckRegistrationParameters"/>.</param>
	IOmexHealthChecksBuilder Add(HealthCheckRegistration registration, HealthCheckRegistrationParameters parameters);
}
