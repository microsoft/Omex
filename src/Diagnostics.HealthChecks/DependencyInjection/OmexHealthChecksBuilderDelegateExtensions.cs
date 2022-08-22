// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable IDE0008 // Use explicit type

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Omex.Preview.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Omex.Preview.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for registering delegates with the <see cref="IOmexHealthChecksBuilder"/>.
/// </summary>
public static class OmexHealthChecksBuilderDelegateExtensions
{
	/// <summary>
	/// Adds a new health check with the specified name and implementation.
	/// </summary>
	/// <param name="builder">The <see cref="IOmexHealthChecksBuilder"/>.</param>
	/// <param name="name">The name of the health check.</param>
	/// <param name="tags">A list of tags that can be used to filter health checks.</param>
	/// <param name="check">A delegate that provides the health check implementation.</param>
	/// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
	/// <param name="parameters">An optional <see cref="HealthCheckRegistrationParameters"/> representing the individual health check options.</param>
	/// <returns>The <see cref="IOmexHealthChecksBuilder"/>.</returns>
	[SuppressMessage("ApiDesign", "RS0026:Do not add multiple public overloads with optional parameters", Justification = "Required to maintain compatibility")]
	public static IOmexHealthChecksBuilder AddCheck(
		this IOmexHealthChecksBuilder builder,
		string name,
		Func<HealthCheckResult> check,
		IEnumerable<string>? tags = default,
		TimeSpan? timeout = default,
		HealthCheckRegistrationParameters? parameters = default)
	{
		if (builder == null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		if (name == null)
		{
			throw new ArgumentNullException(nameof(name));
		}

		if (check == null)
		{
			throw new ArgumentNullException(nameof(check));
		}

		if (parameters == null) // Avoids API source breaking
		{
			return (IOmexHealthChecksBuilder)HealthChecksBuilderDelegateExtensions.AddCheck(builder, name, check, tags, timeout);
		}

		var instance = new DelegateHealthCheck((ct) => new ValueTask<HealthCheckResult>(check()));
		return builder.Add(new HealthCheckRegistration(name, instance, failureStatus: default, tags, timeout), parameters);
	}

	/// <summary>
	/// Adds a new health check with the specified name and implementation.
	/// </summary>
	/// <param name="builder">The <see cref="IOmexHealthChecksBuilder"/>.</param>
	/// <param name="name">The name of the health check.</param>
	/// <param name="tags">A list of tags that can be used to filter health checks.</param>
	/// <param name="check">A delegate that provides the health check implementation.</param>
	/// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
	/// <param name="parameters">An optional <see cref="HealthCheckRegistrationParameters"/> representing the individual health check options.</param>
	/// <returns>The <see cref="IOmexHealthChecksBuilder"/>.</returns>
	[SuppressMessage("ApiDesign", "RS0026:Do not add multiple public overloads with optional parameters", Justification = "Required to maintain compatibility")]
	public static IOmexHealthChecksBuilder AddCheck(
		this IOmexHealthChecksBuilder builder,
		string name,
		Func<CancellationToken, HealthCheckResult> check,
		IEnumerable<string>? tags = default,
		TimeSpan? timeout = default,
		HealthCheckRegistrationParameters? parameters = default)
	{
		if (builder == null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		if (name == null)
		{
			throw new ArgumentNullException(nameof(name));
		}

		if (check == null)
		{
			throw new ArgumentNullException(nameof(check));
		}

		if (parameters == null)
		{
			return (IOmexHealthChecksBuilder)HealthChecksBuilderDelegateExtensions.AddCheck(builder, name, check, tags, timeout);
		}

		var instance = new DelegateHealthCheck((ct) => new ValueTask<HealthCheckResult>(check(ct)));
		return builder.Add(new HealthCheckRegistration(name, instance, failureStatus: default, tags, timeout), parameters);
	}

	/// <summary>
	/// Adds a new health check with the specified name and implementation.
	/// </summary>
	/// <param name="builder">The <see cref="IOmexHealthChecksBuilder"/>.</param>
	/// <param name="name">The name of the health check.</param>
	/// <param name="tags">A list of tags that can be used to filter health checks.</param>
	/// <param name="check">A delegate that provides the health check implementation.</param>
	/// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
	/// <param name="parameters">An optional <see cref="HealthCheckRegistrationParameters"/> representing the individual health check options.</param>
	/// <returns>The <see cref="IOmexHealthChecksBuilder"/>.</returns>
	[SuppressMessage("ApiDesign", "RS0026:Do not add multiple public overloads with optional parameters", Justification = "Required to maintain compatibility")]
	public static IOmexHealthChecksBuilder AddAsyncCheck(
		this IOmexHealthChecksBuilder builder,
		string name,
		Func<ValueTask<HealthCheckResult>> check,
		IEnumerable<string>? tags = default,
		TimeSpan? timeout = default,
		HealthCheckRegistrationParameters? parameters = default)
	{
		if (builder == null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		if (name == null)
		{
			throw new ArgumentNullException(nameof(name));
		}

		if (check == null)
		{
			throw new ArgumentNullException(nameof(check));
		}

		if (parameters == null)
		{
			return (IOmexHealthChecksBuilder)HealthChecksBuilderDelegateExtensions.AddAsyncCheck(builder, name, check().AsTask, tags, timeout);
		}

		var instance = new DelegateHealthCheck((ct) => check());
		return builder.Add(new HealthCheckRegistration(name, instance, failureStatus: default, tags, timeout), parameters);
	}

	/// <summary>
	/// Adds a new health check with the specified name and implementation.
	/// </summary>
	/// <param name="builder">The <see cref="IOmexHealthChecksBuilder"/>.</param>
	/// <param name="name">The name of the health check.</param>
	/// <param name="tags">A list of tags that can be used to filter health checks.</param>
	/// <param name="check">A delegate that provides the health check implementation.</param>
	/// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
	/// <param name="parameters">An optional <see cref="HealthCheckRegistrationParameters"/> representing the individual health check options.</param>
	/// <returns>The <see cref="IOmexHealthChecksBuilder"/>.</returns>
	[SuppressMessage("ApiDesign", "RS0026:Do not add multiple public overloads with optional parameters", Justification = "Required to maintain compatibility")]
	public static IOmexHealthChecksBuilder AddAsyncCheck(
		this IOmexHealthChecksBuilder builder,
		string name,
		Func<CancellationToken, ValueTask<HealthCheckResult>> check,
		IEnumerable<string>? tags = null,
		TimeSpan? timeout = default,
		HealthCheckRegistrationParameters? parameters = default)
	{
		if (builder == null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		if (name == null)
		{
			throw new ArgumentNullException(nameof(name));
		}

		if (check == null)
		{
			throw new ArgumentNullException(nameof(check));
		}

		if (parameters == null)
		{
			return (IOmexHealthChecksBuilder)HealthChecksBuilderDelegateExtensions.AddAsyncCheck(builder, name, ct => check(ct).AsTask(), tags, timeout);
			;
		}

		var instance = new DelegateHealthCheck((ct) => check(ct));
		return builder.Add(new HealthCheckRegistration(name, instance, failureStatus: default, tags, timeout), parameters);
	}
}
