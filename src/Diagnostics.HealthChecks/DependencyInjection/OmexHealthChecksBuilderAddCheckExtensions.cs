// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable IDE0008 // Use explicit type

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Omex.Preview.Extensions.Diagnostics.HealthChecks;
using DynamicallyAccessedMembersAttribute = Microsoft.Omex.Preview.Extensions.Diagnostics.HealthChecks.DynamicallyAccessedMembersAttribute;
using DynamicallyAccessedMemberTypes = Microsoft.Omex.Preview.Extensions.Diagnostics.HealthChecks.DynamicallyAccessedMemberTypes;
using UnconditionalSuppressMessageAttribute = Microsoft.Omex.Preview.Extensions.Diagnostics.HealthChecks.UnconditionalSuppressMessageAttribute;

namespace Microsoft.Omex.Preview.Extensions.DependencyInjection;

/// <summary>
/// Provides basic extension methods for registering <see cref="IHealthCheck"/> instances in an <see cref="IOmexHealthChecksBuilder"/>.
/// </summary>
public static class OmexHealthChecksBuilderAddCheckExtensions
{
	/// <summary>
	/// Adds a new health check with the specified name and implementation.
	/// </summary>
	/// <param name="builder">The <see cref="IOmexHealthChecksBuilder"/>.</param>
	/// <param name="name">The name of the health check.</param>
	/// <param name="instance">An <see cref="IHealthCheck"/> instance.</param>
	/// <param name="failureStatus">
	/// The <see cref="HealthStatus"/> that should be reported when the health check reports a failure. If the provided value
	/// is <c>null</c>, then <see cref="HealthStatus.Unhealthy"/> will be reported.
	/// </param>
	/// <param name="tags">A list of tags that can be used to filter health checks.</param>
	/// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
	/// <param name="parameters">An optional <see cref="HealthCheckRegistrationParameters"/> representing the individual health check options.</param>
	/// <returns>The <see cref="IOmexHealthChecksBuilder"/>.</returns>
	[SuppressMessage("ApiDesign", "RS0026:Do not add multiple public overloads with optional parameters", Justification = "Required to maintain compatibility")]
	public static IOmexHealthChecksBuilder AddCheck(
		this IOmexHealthChecksBuilder builder,
		string name,
		IHealthCheck instance,
		HealthStatus? failureStatus = default,
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

		if (instance == null)
		{
			throw new ArgumentNullException(nameof(instance));
		}

		if (parameters == null)
		{
			return (IOmexHealthChecksBuilder)HealthChecksBuilderAddCheckExtensions.AddCheck(builder, name, instance, failureStatus, tags, timeout);
		}

		return builder.Add(new HealthCheckRegistration(name, instance, failureStatus, tags, timeout), parameters);
	}

	/// <summary>
	/// Adds a new health check with the specified name and implementation.
	/// </summary>
	/// <typeparam name="T">The health check implementation type.</typeparam>
	/// <param name="builder">The <see cref="IOmexHealthChecksBuilder"/>.</param>
	/// <param name="name">The name of the health check.</param>
	/// <param name="failureStatus">
	/// The <see cref="HealthStatus"/> that should be reported when the health check reports a failure. If the provided value
	/// is <c>null</c>, then <see cref="HealthStatus.Unhealthy"/> will be reported.
	/// </param>
	/// <param name="tags">A list of tags that can be used to filter health checks.</param>
	/// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
	/// <param name="parameters">An optional <see cref="HealthCheckRegistrationParameters"/> representing the individual health check options.</param>
	/// <returns>The <see cref="IOmexHealthChecksBuilder"/>.</returns>
	/// <remarks>
	/// This method will use <see cref="ActivatorUtilities.GetServiceOrCreateInstance{T}(IServiceProvider)"/> to create the health check
	/// instance when needed. If a service of type <typeparamref name="T"/> is registered in the dependency injection container
	/// with any lifetime it will be used. Otherwise an instance of type <typeparamref name="T"/> will be constructed with
	/// access to services from the dependency injection container.
	/// </remarks>
	[SuppressMessage("ApiDesign", "RS0027:Public API with optional parameter(s) should have the most parameters amongst its public overloads.", Justification = "Required to maintain compatibility")]
	public static IOmexHealthChecksBuilder AddCheck<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
		this IOmexHealthChecksBuilder builder,
		string name,
		HealthStatus? failureStatus = default,
		IEnumerable<string>? tags = default,
		TimeSpan? timeout = default,
		HealthCheckRegistrationParameters? parameters = default) where T : class, IHealthCheck
	{
		if (builder == null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		if (name == null)
		{
			throw new ArgumentNullException(nameof(name));
		}

		if (parameters == null)
		{
			return (IOmexHealthChecksBuilder)HealthChecksBuilderAddCheckExtensions.AddCheck<T>(builder, name, failureStatus, tags, timeout);
		}

		return builder.Add(new HealthCheckRegistration(name, GetServiceOrCreateInstance, failureStatus, tags, timeout), parameters);

		[UnconditionalSuppressMessage("Trimming", "IL2091",
		   Justification = "DynamicallyAccessedMemberTypes.PublicConstructors is enforced by calling method.")]
		static T GetServiceOrCreateInstance(IServiceProvider serviceProvider) =>
			ActivatorUtilities.GetServiceOrCreateInstance<T>(serviceProvider);
	}

	/// <summary>
	/// Adds a new type activated health check with the specified name and implementation.
	/// </summary>
	/// <typeparam name="T">The health check implementation type.</typeparam>
	/// <param name="builder">The <see cref="IOmexHealthChecksBuilder"/>.</param>
	/// <param name="name">The name of the health check.</param>
	/// <param name="failureStatus">
	/// The <see cref="HealthStatus"/> that should be reported when the health check reports a failure. If the provided value
	/// is <c>null</c>, then <see cref="HealthStatus.Unhealthy"/> will be reported.
	/// </param>
	/// <param name="tags">A list of tags that can be used to filter health checks.</param>
	/// <param name="args">Additional arguments to provide to the constructor.</param>
	/// <param name="timeout">A <see cref="TimeSpan"/> representing the timeout of the check.</param>
	/// <param name="parameters">An optional <see cref="HealthCheckRegistrationParameters"/> representing the individual health check options.</param>
	/// <returns>The <see cref="IOmexHealthChecksBuilder"/>.</returns>
	/// <remarks>
	/// This method will use <see cref="ActivatorUtilities.CreateInstance{T}(IServiceProvider, object[])"/> to create the health check
	/// instance when needed. Additional arguments can be provided to the constructor via <paramref name="args"/>.
	/// </remarks>
	public static IOmexHealthChecksBuilder AddTypeActivatedCheck<
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
		this IOmexHealthChecksBuilder builder,
		string name,
		HealthStatus? failureStatus,
		IEnumerable<string>? tags,
		TimeSpan? timeout,
		HealthCheckRegistrationParameters? parameters,
		params object[] args) where T : class, IHealthCheck
	{
		if (builder == null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		if (name == null)
		{
			throw new ArgumentNullException(nameof(name));
		}

		if (parameters == null)
		{
			return (IOmexHealthChecksBuilder)HealthChecksBuilderAddCheckExtensions.AddTypeActivatedCheck<T>(builder, name, failureStatus, tags ?? new List<string>(), timeout: timeout ?? Timeout.InfiniteTimeSpan);
		}

		return builder.Add(new HealthCheckRegistration(name, CreateInstance, failureStatus, tags, timeout), parameters);

		[UnconditionalSuppressMessage("Trimming", "IL2091",
			Justification = "DynamicallyAccessedMemberTypes.PublicConstructors is enforced by calling method.")]
		T CreateInstance(IServiceProvider serviceProvider) => ActivatorUtilities.CreateInstance<T>(serviceProvider, args);
	}
}
