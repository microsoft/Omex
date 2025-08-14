// Copyright (C) Microsoft Corporation. All rights reserved.

namespace Microsoft.Omex.Extensions.FeatureManagement.Filters;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.FeatureManagement.Extensions;
using Microsoft.Omex.Extensions.FeatureManagement.Filters.Filter.Settings;

/// <summary>
/// The parent filter, which evaluates another feature as a parent condition.
/// </summary>
/// <remarks>The goal of this is to allow inheritance of feature states, thereby reducing the need for duplicate definitions.</remarks>
/// <param name="logger">The logger.</param>
/// <param name="serviceProvider">The service provider.</param>
[FilterAlias("Parent")]
public sealed class ParentFilter(
	ILogger<ParentFilter> logger,
	IServiceProvider serviceProvider) : IFeatureFilter
{
	/// <inheritdoc />
	public async Task<bool> EvaluateAsync(FeatureFilterEvaluationContext featureFilterContext)
	{
		// IExtendedFeatureManager cannot be resolved via the constructor as the feature filter will be created
		// before the ExtendedFeatureManager is registered in the service provider. This ordering is a side effect
		// of the way the Feature Management library initializes filters.
		IExtendedFeatureManager? extendedFeatureManager = serviceProvider.GetService<IExtendedFeatureManager>();
		if (extendedFeatureManager == null)
		{
			logger.LogError(Tag.Create(), $"{nameof(ParentFilter)} could not resolve the {nameof(IExtendedFeatureManager)} object. Ensure the object is correctly registered via dependency injection.");
			return false;
		}

		ParentFilterSettings settings = featureFilterContext.Parameters.GetOrCreate<ParentFilterSettings>();
		if (string.IsNullOrWhiteSpace(settings.Feature))
		{
			return false;
		}

		return await extendedFeatureManager.IsEnabledAsync(settings.Feature);
	}
}
