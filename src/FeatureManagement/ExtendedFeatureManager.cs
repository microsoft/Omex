// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.Omex.Extensions.Abstractions;

/// <summary>
/// The extended feature manager provides support for server-controlled feature overrides.
/// </summary>
/// <param name="featureManager">The feature manger.</param>
/// <param name="logger">The logger.</param>
/// <param name="settings">The settings.</param>
internal sealed class ExtendedFeatureManager(
	IFeatureManager featureManager,
	ILogger<ExtendedFeatureManager> logger,
	IOptionsMonitor<FeatureOverrideSettings> settings) : IExtendedFeatureManager
{
	/// <inheritdoc/>
	public bool? GetOverride(string feature)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(feature);

		const string methodName = $"{nameof(ExtendedFeatureManager)}.{nameof(GetOverride)}";

		logger.LogInformation(Tag.Create(), $"{methodName} checking '{{Feature}}'.", feature);

		// Checks if the feature is disabled in the settings, which would always turn the feature off.
		if (settings.CurrentValue.Disabled.Contains(feature, StringComparer.OrdinalIgnoreCase))
		{
			logger.LogInformation(Tag.Create(), $"{methodName} returned false for '{{Feature}}' as it is overridden in the {nameof(settings.CurrentValue.Disabled)} setting.", feature);
			return false;
		}

		// Checks if the feature is enabled in the settings, which would always turn the feature on.
		if (settings.CurrentValue.Enabled.Contains(feature, StringComparer.OrdinalIgnoreCase))
		{
			logger.LogInformation(Tag.Create(), $"{methodName} returned true for '{{Feature}}' as it is overridden in the {nameof(settings.CurrentValue.Enabled)} setting.", feature);
			return true;
		}

		return null;
	}

	/// <inheritdoc/>
	public IAsyncEnumerable<string> GetFeatureNamesAsync() =>
		featureManager.GetFeatureNamesAsync();

	/// <inheritdoc/>
	public Task<bool> IsEnabledAsync(string feature) =>
		IsEnabledInternalAsync<object?>(feature, null);

	///<inheritdoc/>
	public Task<bool> IsEnabledAsync<TContext>(string feature, TContext context) =>
		IsEnabledInternalAsync(feature, context);

	private async Task<bool> IsEnabledInternalAsync<TContext>(string feature, TContext? context)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(feature);

		const string methodName = $"{nameof(ExtendedFeatureManager)}.{nameof(IsEnabledAsync)}";

		logger.LogInformation(Tag.Create(), $"{methodName} checking '{{Feature}}'.", feature);
		bool? overrideValue = GetOverride(feature);
		bool result;
		if (overrideValue.HasValue)
		{
			result = overrideValue.GetValueOrDefault();
		}
		else
		{
			result = context is null
				? await featureManager.IsEnabledAsync(feature)
				: await featureManager.IsEnabledAsync(feature, context);
		}

		logger.LogInformation(Tag.Create(), $"{methodName} returned '{{IsEnabled}}' for '{{Feature}}'.", result, feature);
		return result;
	}
}
