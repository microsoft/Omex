// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Microsoft.Omex.Extensions.FeatureManagement.Experimentation;
using Microsoft.Omex.Extensions.FeatureManagement.Filters;
using Microsoft.Omex.Extensions.FeatureManagement.Filters.Configuration;

/// <summary>
/// Extension methods for <see cref="ServiceCollectionExtensions"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers the <see cref="ExtendedFeatureManager"/> in the <see cref="IServiceCollection"/> along with any custom filters.
	/// </summary>
	/// <remarks>To provide concrete implementations of <see cref="IExperimentManager"/> and <see cref="IIPRangeProvider"/>,
	/// call <c>AddSingleton</c> after this method.</remarks>
	/// <param name="services">The <see cref="IServiceCollection"/> to which to register the classes.</param>
	/// <param name="configuration">The configuration.</param>
	/// <returns>The updated <see cref="IServiceCollection"/>.</returns>
	public static IServiceCollection AddOmexFeatureManagement(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddOptions<FeatureOverrideSettings>()
			.Bind(configuration.GetSection(nameof(FeatureOverrideSettings)))
			.ValidateDataAnnotations();

		services.AddSingleton<IExperimentManager, EmptyExperimentManager>();
		services.AddSingleton<IIPRangeProvider, EmptyIPRangeProvider>();

		services.AddFeatureManagement()
			.AddFeatureFilter<CampaignFilter>()
			.AddFeatureFilter<ToggleFilter>()
			.AddFeatureFilter<IPAddressFilter>()
			.AddFeatureFilter<MarketFilter>()
			.AddFeatureFilter<ParentFilter>();

		services.AddScoped<IExtendedFeatureManager, ExtendedFeatureManager>();
		services.AddScoped<IFeatureGatesConsolidator, FeatureGatesConsolidator>();
		services.AddScoped<IFeatureGatesService, FeatureGatesService>();

		return services;
	}
}
