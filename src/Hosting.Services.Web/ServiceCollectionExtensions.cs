// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares;
using Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares.UserIdentity.Options;

namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Extension to add Omex dependencies to IServiceCollection
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		private static IServiceCollection AddMiddlewares(this IServiceCollection services)
		{
			services
				.AddSingleton<UserHashIdentityMiddleware>()
				.AddSingleton<ActivityEnrichmentMiddleware>()
				.AddSingleton<ResponseHeadersMiddleware>();

#pragma warning disable CS0618 // We need to register all middlewares, even if obsolete
			services.AddSingleton<ObsoleteCorrelationHeadersMiddleware>();
#pragma warning restore CS0618

			return services;
		}

		private static IServiceCollection AddDefaultProviders(this IServiceCollection services)
		{
			services.TryAddSingleton<ISystemClock, SystemClock>();
			services.TryAddSingleton<EmptySaltProvider>();
			services.TryAddSingleton<RotatingSaltProvider>();
			services.TryAddEnumerable(ServiceDescriptor.Singleton<IUserIdentityProvider, IpBasedUserIdentityProvider>());

			static ISaltProvider getSaltProvider(IServiceProvider provider) =>
				provider.GetRequiredService<IOptions<UserIdentiyMiddlewareOptions>>().Value.LoggingCompliance switch
				{
					UserIdentiyComplianceLevel.LiveId => provider.GetRequiredService<EmptySaltProvider>(),
					_ => provider.GetRequiredService<RotatingSaltProvider>()
				};

			services.TryAddTransient(getSaltProvider);
			return services;
		}
		/// <summary>
		/// Register types for Omex middleware
		/// </summary>
		public static IServiceCollection AddOmexMiddleware(this IServiceCollection services)
		{
			services.AddDefaultProviders();
			services.AddMiddlewares();

			return services;
		}

		/// <summary>
		/// Register static salt provider, needs to be called before AddDefaultProviders to take effect
		/// </summary>
		public static IServiceCollection AddStaticSaltProvider(this IServiceCollection services)
		{
			services.AddOptions<StaticSaltProviderOptions>();
			services.AddTransient<ISaltProvider, StaticSaltProvider>();

			return services;
		}

		/// <summary>
		/// Register email based identity provider, needs to be called after AddDefaultProviders to take effect
		/// </summary>
		public static IServiceCollection AddEmailBasedIdentityProvider(this IServiceCollection services)
		{
			services.TryAddEnumerable(ServiceDescriptor.Singleton<IUserIdentityProvider, EmailBasedUserIdentityProvider>());

			return services;
		}
	}
}
