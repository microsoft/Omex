// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.FeatureManagement.Constants;
using Microsoft.Omex.Extensions.FeatureManagement.Experimentation;

namespace Microsoft.Omex.Extensions.FeatureManagement
{
	/// <summary>
	/// The service managing the feature gates.
	/// </summary>
	/// <param name="activitySource">The activity source.</param>
	/// <param name="experimentManager">The experiment manager.</param>
	/// <param name="featureManager">The feature manager.</param>
	internal sealed class FeatureGatesService(
		ActivitySource activitySource,
		IExperimentManager experimentManager,
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
		public async Task<IDictionary<string, bool>> GetFeatureGatesAsync()
		{
			using Activity? activity = activitySource
				.StartActivity(FeatureManagementActivityNames.FeatureGatesService.GetFeatureGatesAsync)?
				.MarkAsSystemError();

			Dictionary<string, bool> featureGates = new(StringComparer.OrdinalIgnoreCase);
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

		/// <inheritdoc />
		public async Task<bool> IsExperimentApplicableAsync(string featureGate, ExperimentFilters filters, CancellationToken cancellationToken)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(featureGate);

			using Activity? activity = activitySource
				.StartActivity(FeatureManagementActivityNames.FeatureGatesService.IsExperimentApplicableAsync)?
				.MarkAsExpectedError();

			bool? queryParamOverride = featureManager.GetOverride(featureGate);
			if (queryParamOverride.HasValue)
			{
				activity?.SetMetadata($"FromOverride_{featureGate}").MarkAsSuccess();
				return queryParamOverride.Value;
			}

			IDictionary<string, bool> features = await experimentManager.GetExperimentStatusesAsync(filters, cancellationToken);
			bool isKeyPresent = features.ContainsKey(featureGate);
			if (!isKeyPresent)
			{
				if (await featureManager.IsEnabledAsync(featureGate))
				{
					activity?.SetMetadata($"FromFeatureManager_{featureGate}").MarkAsSuccess();
					return true;
				}

				return false;
			}

			activity?.SetMetadata($"FromExperiment_{featureGate}").MarkAsSuccess();
			return features[featureGate];
		}

		private static void UpdateFeatureMap(Dictionary<string, bool> featureGates, IEnumerable<string> features, bool overrideValue)
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
