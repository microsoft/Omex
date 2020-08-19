// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web
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
			services
				.AddSingleton<UserIdentiyMiddleware>()
				.AddSingleton<ActivityEnrichmentMiddleware>()
				.AddSingleton<ResponseHeadersMiddleware>();

#pragma warning disable CS0618 // We need to register all middlewares, even if obsolete
			services.AddSingleton<ObsoleteCorrelationHeadersMiddleware>();
#pragma warning restore CS0618

			return services;
		}

		internal static IServiceCollection PropagateRequired<TValue>(this IServiceCollection services, IServiceProvider provider)
			where TValue : class
				=> services.AddSingleton(provider.GetRequiredService<TValue>());

		internal static IServiceCollection PropagateOptional<TValue>(this IServiceCollection services, IServiceProvider provider)
			where TValue : class
		{
			TValue? stateAccessors = provider.GetService<TValue>();
			if (stateAccessors != null)
			{
				services.AddSingleton(stateAccessors);
			}

			return services;
		}
	}
}
