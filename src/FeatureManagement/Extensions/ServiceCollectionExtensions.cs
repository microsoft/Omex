// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.Extensions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.Omex.Extensions.FeatureManagement.Authentication;
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
	/// <param name="services">The <see cref="IServiceCollection"/> to which to register the classes.</param>
	/// <param name="configuration">The configuration.</param>
	/// <param name="customerIdProvider">The customer ID provider. If <c>null</c>, the <see cref="EntraIdProvider"/> will be used.</param>
	/// <param name="experimentManager">The experiment manager. If <c>null</c>, the <see cref="EmptyExperimentManager"/> will be used.</param>
	/// <param name="ipRangeProvider">The IP range provider. If <c>null</c>, the <see cref="EmptyIPRangeProvider"/> will be used.</param>
	/// <returns>The updated <see cref="IServiceCollection"/>.</returns>
	public static IServiceCollection ConfigureFeatureManagement(
		this IServiceCollection services,
		IConfiguration configuration,
		ICustomerIdProvider? customerIdProvider = null,
		IExperimentManager? experimentManager = null,
		IIPRangeProvider? ipRangeProvider = null)
	{
		services.AddOptions<FeatureOverrideSettings>()
			.Bind(configuration.GetSection(nameof(FeatureOverrideSettings)))
			.ValidateDataAnnotations();

		services.AddSingleton(provider =>
			customerIdProvider ?? new EntraIdProvider(
				provider.GetRequiredService<IHttpContextAccessor>(),
				provider.GetRequiredService<ILogger<EntraIdProvider>>()));
		services.AddSingleton(experimentManager ?? new EmptyExperimentManager());
		services.AddSingleton(ipRangeProvider ?? new EmptyIPRangeProvider());

		services.AddFeatureManagement()
			.AddFeatureFilter<CampaignFilter>()
			.AddFeatureFilter<ToggleFilter>()
			.AddFeatureFilter<IPAddressFilter>()
			.AddFeatureFilter<MarketFilter>()
			.AddFeatureFilter<ParentFilter>()
			.AddFeatureFilter<RolloutFilter>();

		services.AddScoped<IExtendedFeatureManager, ExtendedFeatureManager>();
		services.AddScoped<IFeatureGatesConsolidator, FeatureGatesConsolidator>();
		services.AddScoped<IFeatureGatesService, FeatureGatesService>();

		return services;
	}
}
