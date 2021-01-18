// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares;

namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Extension to add Omex dependencies to IServiceCollection
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Register types for Omex middleware
		/// </summary>
		public static IServiceCollection AddOmexMiddleware(this IServiceCollection services)
		{
			services.TryAddSingleton<ISystemClock, SystemClock>();
			services.TryAddSingleton<EmptySaltProvider>();
			services.TryAddSingleton<RotatingSaltProvider>();

			static ISaltProvider getSaltProvider(IServiceProvider provider) =>
				provider.GetRequiredService<IOptions<UserIdentiyMiddlewareOptions>>().Value.LoggingComlience switch
				{
					UserIdentiyComplianceLevel.LiveId => provider.GetRequiredService<EmptySaltProvider>(),
					_ => provider.GetRequiredService<RotatingSaltProvider>()
				};

			services.TryAddTransient(getSaltProvider);

			services
				.AddSingleton<UserIdentiyMiddleware>()
				.AddSingleton<ActivityEnrichmentMiddleware>()
				.AddSingleton<ResponseHeadersMiddleware>();

#pragma warning disable CS0618 // We need to register all middlewares, even if obsolete
			services.AddSingleton<ObsoleteCorrelationHeadersMiddleware>();
#pragma warning restore CS0618

			return services;
		}
	}
}
