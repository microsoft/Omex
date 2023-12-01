// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Diagnostics.HealthChecks.Composables;
using Microsoft.Omex.Extensions.Hosting.Certificates;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks
{
	/// <summary>
	/// Extension methods to configure <see cref="CertificatesValidityHealthCheck"/>.
	/// </summary>
	public static class CertificatesValidityHealthCheckBuilderExtensions
	{
		/// <summary>
		/// Registers <see cref="CertificatesValidityHealthCheck"/> as an observable health check.
		/// </summary>
		/// <param name="builder">The <see cref="IHealthChecksBuilder"/> to add HealthCheck registration to.</param>
		/// <param name="parameters">
		/// The health check parameters passed at DI configuration time.
		/// They can be used to specify escalation configuration (e.g. the IcM ticket severity, owning team).
		/// </param>
		/// <param name="certificateReaderFactory">
		/// An optional factory to obtain an <see cref="ICertificateReader"/> instance.
		/// If not provided, it is resolved from <see cref="IServiceProvider"/>.
		/// </param>
		/// <param name="loggerFactory">
		/// An optional factory to obtain an <see cref="ILogger{CertificatesValidityHealthCheck}"/> instance.
		/// If not provided, it is resolved from <see cref="IServiceProvider"/>.
		/// </param>
		/// <param name="optionsFactory">
		/// An optional factory to obtain an <see cref="IOptions{CertificatesValidityHealthCheckOptions}"/> instance.
		/// If not provided, it is resolved from <see cref="IServiceProvider"/>.
		/// </param>
		/// <param name="activitySourceFactory">
		/// An optional factory to obtain an <see cref="ActivitySource"/> instance.
		/// If not provided, it is resolved from <see cref="IServiceProvider"/>.
		/// </param>
		/// <param name="healthCheckName">The health check name. When not provided, the name of the health check is set to 'CertificatesValidityHealthCheck'.</param>
		/// <param name="failureStatus">
		/// The <see cref="HealthStatus"/> that should be reported when the health check fails. When not provided,
		/// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
		/// </param>
		/// <param name="tags">An optional list of tags that can be used to filter sets of health checks.</param>
		/// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
		/// <returns>The specified <paramref name="builder"/>.</returns>
		public static IHealthChecksBuilder AddCertificatesValidity(
			this IHealthChecksBuilder builder,
			HealthCheckParameters parameters,
			Func<IServiceProvider, ICertificateReader>? certificateReaderFactory = default,
			Func<IServiceProvider, ILogger<CertificatesValidityHealthCheck>>? loggerFactory = default,
			Func<IServiceProvider, IOptions<CertificatesValidityHealthCheckOptions>>? optionsFactory = default,
			Func<IServiceProvider, ActivitySource>? activitySourceFactory = default,
			string? healthCheckName = default,
			HealthStatus? failureStatus = default,
			IEnumerable<string>? tags = default,
			TimeSpan? timeout = default)
		{
			builder.Add(new HealthCheckRegistration(
				string.IsNullOrWhiteSpace(healthCheckName) ? "CertificatesValidityHealthCheck" : healthCheckName!,
				sp => new CertificatesValidityHealthCheck(
					parameters,
					optionsFactory?.Invoke(sp) ?? sp.GetRequiredService<IOptions<CertificatesValidityHealthCheckOptions>>(),
					loggerFactory?.Invoke(sp) ?? sp.GetRequiredService<ILogger<CertificatesValidityHealthCheck>>(),
					certificateReaderFactory?.Invoke(sp) ?? sp.GetRequiredService<ICertificateReader>()
					)
				.AsObservableHealthCheck(
					activitySourceFactory?.Invoke(sp) ?? sp.GetRequiredService<ActivitySource>(),
					loggerFactory?.Invoke(sp) ?? sp.GetRequiredService<ILogger<CertificatesValidityHealthCheck>>(),
					parameters
				),
				failureStatus,
				tags,
				timeout));

			return builder;
		}
	}
}
