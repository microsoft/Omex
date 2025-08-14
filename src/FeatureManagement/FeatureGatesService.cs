// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.FeatureManagement.Constants;

namespace Microsoft.Omex.Extensions.FeatureManagement
{
	/// <summary>
	/// The service managing the feature gates.
	/// </summary>
	/// <param name="activitySource">The activity source.</param>
	/// <param name="featureManager">The feature manager.</param>
	internal sealed class FeatureGatesService(
		ActivitySource activitySource,
		IExtendedFeatureManager featureManager) : IFeatureGatesService
	{
		private const string FrontendFeaturePrefix = "FE_";

		/// <inheritdoc />
		public string RequestedFeatures =>
			featureManager.EnabledFeatures;

		/// <inheritdoc/>
		public string BlockedFeatures =>
			featureManager.DisabledFeatures;

		/// <inheritdoc/>
		public async Task<IDictionary<string, object>> GetFeatureGatesAsync()
		{
			using Activity? activity = activitySource
				.StartActivity(FeatureManagementActivityNames.FeatureGatesService.GetFeatureGatesAsync)?
				.MarkAsSystemError();

			Dictionary<string, object> featureGates = new(StringComparer.OrdinalIgnoreCase);
			await foreach (string feature in featureManager.GetFeatureNamesAsync())
			{
				if (feature.StartsWith(FrontendFeaturePrefix, StringComparison.OrdinalIgnoreCase))
				{
					bool featureFlag = await featureManager.IsEnabledAsync(feature);
					featureGates.TryAdd(feature.Substring(FrontendFeaturePrefix.Length), featureFlag);
				}
			}

			UpdateFeatureMap(featureGates, featureManager.EnabledFeaturesList, true);
			UpdateFeatureMap(featureGates, featureManager.DisabledFeaturesList, false);

			activity?.MarkAsSuccess();
			return featureGates;
		}

		/// <inheritdoc/>
		public async Task<bool> IsFeatureGateApplicableAsync(string featureGate)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(featureGate);

			using Activity? activity = activitySource
				.StartActivity(FeatureManagementActivityNames.FeatureGatesService.IsFeatureGateApplicableAsync)?
				.MarkAsSystemError();

			bool result = await featureManager.IsEnabledAsync(featureGate);

			activity?.MarkAsSuccess();
			return result;
		}

		private static void UpdateFeatureMap(Dictionary<string, object> featureGates, IEnumerable<string> features, bool overrideValue)
		{
			foreach (string feature in features)
			{
				string adjustedFeature = feature;
				if (feature.StartsWith(FrontendFeaturePrefix, StringComparison.OrdinalIgnoreCase))
				{
					adjustedFeature = feature.Substring(FrontendFeaturePrefix.Length);
				}

				featureGates[adjustedFeature] = overrideValue;
			}
		}
	}
}
