// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;
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
	/// <param name="logger">The logger.</param>
	internal sealed class FeatureGatesService(
		ActivitySource activitySource,
		IExperimentManager experimentManager,
		IExtendedFeatureManager featureManager,
		ILogger<FeatureGatesService> logger) : IFeatureGatesService
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
		public async Task<IDictionary<string, object>> GetExperimentalFeaturesAsync(IDictionary<string, object> filters, CancellationToken cancellationToken)
		{
			using Activity? activity = activitySource
				.StartActivity(FeatureManagementActivityNames.FeatureGatesService.GetExperimentalFeaturesAsync)?
				.MarkAsSystemError();

			IEnumerable<string> filtersUsed = filters.Select(item => $"{item.Key}:{item.Value}");
			logger.LogInformation(Tag.Create(), $"{nameof(FeatureGatesService)}.{nameof(GetExperimentalFeaturesAsync)} allocating experiment with the following filters: {{Filters}}", string.Join(',', filtersUsed));

			IDictionary<string, object> response = await experimentManager.GetFlightsAsync(filters, cancellationToken);
			activity?.MarkAsSuccess();
			return response;
		}

		/// <inheritdoc />
		public async Task<FeatureGateResult> GetExperimentFeatureValueAsync(string featureGate, IDictionary<string, object> filters, CancellationToken cancellationToken)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(featureGate);

			IDictionary<string, object> features = await GetExperimentalFeaturesAsync(filters, cancellationToken);
			if (!features.TryGetValue(featureGate, out object? value))
			{
				return new(false);
			}

			string? featureGateValue = value.ToString();
			if (string.IsNullOrWhiteSpace(featureGateValue))
			{
				return new(false);
			}

			if (bool.TryParse(featureGateValue, out bool valueAsBool))
			{
				return new(valueAsBool);
			}

			// We received a feature gate value for the experiment, so the customer user is allocated. The value could
			// not be parsed so assume that the values have alternative meanings. For this to work, experiments should
			// be setup to return "false" for the control feature gate.
			return new(true, featureGateValue);
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
		public async Task<bool> IsExperimentApplicableAsync(string featureGate, IDictionary<string, object> filters, CancellationToken cancellationToken)
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

			IDictionary<string, object> features = await GetExperimentalFeaturesAsync(filters, cancellationToken);
			bool isKeyPresent = features.ContainsKey(featureGate);
			string? featureGateValue = isKeyPresent ? features[featureGate].ToString() : string.Empty;
			if (string.IsNullOrWhiteSpace(featureGateValue))
			{
				if (await featureManager.IsEnabledAsync(featureGate))
				{
					activity?.SetMetadata($"FromFeatureManager_{featureGate}").MarkAsSuccess();
					return true;
				}

				return false;
			}

			activity?.SetMetadata($"FromExperiment_{featureGate}").MarkAsSuccess();
			if (!bool.TryParse(featureGateValue, out bool variable))
			{
				// We received a feature gate value for the experiment, so the customer user is allocated. The value could
				// not be parsed so assume that the values have alternative meanings. For this to work, experiments should
				// be setup to return "false" for the control feature gate.
				return true;
			}

			return variable;
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
