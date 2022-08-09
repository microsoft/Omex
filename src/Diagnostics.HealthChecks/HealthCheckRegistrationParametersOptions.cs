// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// Represent the individual health check options associated with an <see cref="T:Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistration"/>.
/// </summary>
public sealed class HealthCheckRegistrationParametersOptions
{
	/// <summary>
	/// Gets the health check registrations.
	/// </summary>
	public IDictionary<string, HealthCheckRegistrationParameters> RegistrationParameters { get; } = new Dictionary<string, HealthCheckRegistrationParameters>();
}
