// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.FeatureManagement.Constants;
using Microsoft.Omex.Extensions.FeatureManagement.Extensions;

namespace Microsoft.Omex.Extensions.FeatureManagement
{
	/// <summary>
	/// The extended feature manager provides support for dynamic overriding or toggling of features.
	/// </summary>
	/// <param name="featureManager">The feature manger.</param>
	/// <param name="httpContextAccessor">The HTTP context accessor.</param>
	/// <param name="logger">The logger.</param>
	/// <param name="settings">The settings.</param>
	internal sealed class ExtendedFeatureManager(
		IFeatureManager featureManager,
		IHttpContextAccessor httpContextAccessor,
		ILogger<ExtendedFeatureManager> logger,
		IOptionsMonitor<FeatureOverrideSettings> settings) : IExtendedFeatureManager
	{
		/// <inheritdoc/>
		public string DisabledFeatures =>
			httpContextAccessor.GetParameter(RequestParameters.Query.DisabledFeatures);

		/// <inheritdoc/>
		public string[] DisabledFeaturesList =>
			SplitFeatures(DisabledFeatures);

		/// <inheritdoc/>
		public string EnabledFeatures =>
			httpContextAccessor.GetParameter(RequestParameters.Query.EnabledFeatures);

		/// <inheritdoc/>
		public string[] EnabledFeaturesList =>
			SplitFeatures(EnabledFeatures);

		/// <inheritdoc/>
		public bool? GetOverride(string feature)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(feature);

			logger.LogInformation(Tag.Create(), $"{nameof(ExtendedFeatureManager)}.{nameof(GetOverride)} checking '{{Feature}}'.", feature);

			// Each option below ignores any filters applied to the feature.

			// Checks for the feature in the disabled feature query string, which would always turn the feature off.
			if (DisabledFeaturesList.Contains(feature, StringComparer.OrdinalIgnoreCase))
			{
				logger.LogInformation(Tag.Create(), $"{nameof(ExtendedFeatureManager)}.{nameof(GetOverride)} returned false for '{{Feature}}' as it is switched off via the query-string parameter '{{DisableFeatures}}'.", feature, RequestParameters.Query.DisabledFeatures);
				return false;
			}

			// Checks if the feature is disabled in the settings, which would always turn the feature off.
			if (settings.CurrentValue.Disabled.Contains(feature, StringComparer.OrdinalIgnoreCase))
			{
				logger.LogInformation(Tag.Create(), $"{nameof(ExtendedFeatureManager)}.{nameof(GetOverride)} returned true for '{{Feature}}' as it is overridden in the {nameof(settings.CurrentValue.Disabled)} setting.", feature);
				return false;
			}

			// Checks for the feature in the enabled feature query string, which would always turn the feature on.
			if (EnabledFeaturesList.Contains(feature, StringComparer.OrdinalIgnoreCase))
			{
				logger.LogInformation(Tag.Create(), $"{nameof(ExtendedFeatureManager)}.{nameof(GetOverride)} returned true for '{{Feature}}' as it is switched on via the query-string parameter '{{EnabledFeatures}}'.", feature, RequestParameters.Query.EnabledFeatures);
				return true;
			}

			// Checks if the feature is enabled in the settings, which would always turn the feature on.
			if (settings.CurrentValue.Enabled.Contains(feature, StringComparer.OrdinalIgnoreCase))
			{
				logger.LogInformation(Tag.Create(), $"{nameof(ExtendedFeatureManager)}.{nameof(GetOverride)} returned true for '{{Feature}}' as it is overridden in the {nameof(settings.CurrentValue.Enabled)} setting.", feature);
				return true;
			}

			return null;
		}

		/// <inheritdoc/>
		public IAsyncEnumerable<string> GetFeatureNamesAsync() =>
			featureManager.GetFeatureNamesAsync();

		/// <inheritdoc/>
		public async Task<bool> IsEnabledAsync(string feature)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(feature);

			logger.LogInformation(Tag.Create(), $"{nameof(ExtendedFeatureManager)}.{nameof(IsEnabledAsync)} checking '{{Feature}}'.", feature);

			bool? overrideValue = GetOverride(feature);
			bool result = overrideValue.HasValue
				? overrideValue.GetValueOrDefault()
				: await featureManager.IsEnabledAsync(feature);
			logger.LogInformation(Tag.Create(), $"{nameof(ExtendedFeatureManager)}.{nameof(IsEnabledAsync)} returned {{IsEnabled}} for '{{Feature}}'.", result, feature);
			return result;
		}

		///<inheritdoc/>
		public Task<bool> IsEnabledAsync<TContext>(string feature, TContext context)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(feature);

			return featureManager.IsEnabledAsync(feature, context);
		}

		private static string[] SplitFeatures(string features) =>
			string.IsNullOrWhiteSpace(features)
				? []
				: features.Split(';', StringSplitOptions.RemoveEmptyEntries);
	}
}
