// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Omex.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Omex.Extensions.DependencyInjection;

internal sealed class OmexHealthChecksBuilder : IOmexHealthChecksBuilder
{
	public OmexHealthChecksBuilder(IServiceCollection services)
	{
		Services = services;
	}

	public IServiceCollection Services { get; }

	IHealthChecksBuilder IHealthChecksBuilder.Add(HealthCheckRegistration registration)
	{
		if (registration == null)
		{
			throw new ArgumentNullException(nameof(registration));
		}

		Services.Configure<HealthCheckServiceOptions>(options =>
		{
			options.Registrations.Add(registration);
		});

		return this;
	}

	public IOmexHealthChecksBuilder Add(HealthCheckRegistration registration, HealthCheckRegistrationParameters parameters)
	{
		if (registration == null)
		{
			throw new ArgumentNullException(nameof(registration));
		}

		if (parameters == null)
		{
			throw new ArgumentNullException(nameof(parameters));
		}

		Services.Configure<HealthCheckServiceOptions>(options =>
		{
			options.Registrations.Add(registration);
		});

		Services.Configure<HealthCheckRegistrationParametersOptions>(options =>
		{
			options.RegistrationParameters.Add(registration.Name, parameters);
		});

		return this;
	}
}
