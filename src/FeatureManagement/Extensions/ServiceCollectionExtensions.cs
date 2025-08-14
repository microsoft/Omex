// Copyright (C) Microsoft Corporation. All rights reserved.

namespace Microsoft.Omex.Extensions.FeatureManagement.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
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
	/// <param name="services">The <see cref="IServiceCollection"/> to which to register the classes.</param>
	/// <param name="configuration">The configuration.</param>
	/// <param name="ipRangeProvider">The IP range provider. If null, the <see cref="EmptyIPRangeProvider"/> will be used.</param>
	/// <returns>The updated <see cref="IServiceCollection"/>.</returns>
	public static IServiceCollection ConfigureFeatureManagement(
		this IServiceCollection services,
		IConfiguration configuration,
		IIPRangeProvider? ipRangeProvider = null)
	{
		services.AddOptions<FeatureOverrideSettings>()
			.Bind(configuration.GetSection(nameof(FeatureOverrideSettings)))
			.ValidateDataAnnotations();

		// Register the IP range provider.
		services.AddSingleton<IIPRangeProvider>(ipRangeProvider ?? new EmptyIPRangeProvider());

		services.AddFeatureManagement()
			.AddFeatureFilter<CampaignFilter>()
			.AddFeatureFilter<EnvironmentFilter>()
			.AddFeatureFilter<ToggleFilter>()
			.AddFeatureFilter<IPAddressFilter>()
			.AddFeatureFilter<MarketFilter>()
			.AddFeatureFilter<ParentFilter>()
			.AddFeatureFilter<RolloutFilter>();

		services.AddScoped<IExtendedFeatureManager, ExtendedFeatureManager>();
		services.AddScoped<IFeatureGatesService, FeatureGatesService>();

		return services;
	}
}
