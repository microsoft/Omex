// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.v

#pragma warning disable IDE0008 // Use explicit type

using System;
using System.Collections.Generic;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// Represent the individual health check parameters associated with an <see cref="T:Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistration"/>.
/// </summary>
public class HealthCheckRegistrationParameters
{
	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistrationParameters" />.
	/// </summary>
	/// <param name="name">A <see cref="string"/> indicating the name of the check.</param>
	/// <param name="isEnabled">An optional <see cref="bool"/> indicating whether the check should be run.</param>
	/// <param name="delay">An optional <see cref="TimeSpan"/> representing the initial delay applied after the application starts before executing the check.</param>
	/// <param name="period">An optional <see cref="TimeSpan"/> representing the individual period of the check.</param>
	/// <param name="timeout">An optional <see cref="TimeSpan"/> representing the individual timeout of the check.</param>
	public HealthCheckRegistrationParameters(string name, bool isEnabled = true, TimeSpan? delay = default, TimeSpan? period = default, TimeSpan? timeout = default)
	{
		Name = name;
		IsEnabled = isEnabled;
		Delay = delay;
		Period = period;
		Timeout = timeout;
	}

	/// <summary>
	/// Gets the name of the health check.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Gets the initial individual delay applied to the
	/// individual health check after the application starts before executing.
	/// The delay is applied once at startup, and does
	/// not apply to subsequent iterations.
	/// </summary>
	public TimeSpan? Delay { get; }

	/// <summary>
	/// Gets the individual period used for the health check.
	/// </summary>
	public TimeSpan? Period { get; }

	/// <summary>
	/// Gets the timeout for executing the health check.
	/// Use <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> to execute with no timeout.
	/// </summary>
	public TimeSpan? Timeout { get; }

	/// <summary>
	/// Gets or sets whether the health check should be run. Enabled by default.
	/// </summary>
	public bool IsEnabled { get; set; } = true;

	/// <summary>
	/// Gets the property bag associated with the <see cref="HealthCheckRegistrationParameters"/>.
	/// </summary>
	public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();
}
