// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement;

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

/// <summary>
/// The service managing the feature gates.
/// </summary>
/// <remarks>
/// This implementation of IFeatureGatesService provides the core functionality for managing feature gates and
/// experiments. It integrates with:
///
/// - IExtendedFeatureManager: For static feature configuration and query-string overrides
/// - IExperimentManager: For dynamic, customer-targeted experimental features
/// - ActivitySource: For distributed tracing and observability
/// - ILogger: For diagnostic logging
///
/// The service handles the "FE_" prefix convention for frontend features, automatically stripping this prefix when
/// returning feature gates to clients. It also supports feature overrides through query-string parameters, which is
/// useful for testing and debugging.
/// </remarks>
/// <param name="activitySource">The activity source for distributed tracing.</param>
/// <param name="experimentManager">The experiment manager for retrieving experimental features.</param>
/// <param name="featureManager">The extended feature manager for static features and overrides.</param>
/// <param name="logger">The logger for diagnostic information.</param>
internal sealed class FeatureGatesService(
	ActivitySource activitySource,
	IExperimentManager experimentManager,
	IExtendedFeatureManager featureManager,
	ILogger<FeatureGatesService> logger) : IFeatureGatesService
{
	/// <summary>
	/// The prefix used to identify frontend-specific features. Features with this prefix will have it stripped when
	/// returned to clients.
	/// </summary>
	private const string FrontendFeaturePrefix = "FE_";

	/// <inheritdoc />
	public string RequestedFeatures =>
		featureManager.EnabledFeatures;

	/// <inheritdoc/>
	public string BlockedFeatures =>
		featureManager.DisabledFeatures;

	/// <inheritdoc/>
	/// <remarks>
	/// Implementation details:
	/// 1. Iterates through all feature names from the feature manager asynchronously
	/// 2. Filters for features starting with "FE_" prefix (case-insensitive)
	/// 3. Evaluates each frontend feature using IsEnabledAsync
	/// 4. Strips the "FE_" prefix when adding to the result dictionary
	/// 5. Applies overrides from EnabledFeaturesList and DisabledFeaturesList
	/// 6. Uses TryAdd to prevent duplicate keys (first value wins for case-insensitive duplicates)
	/// </remarks>
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
	/// <remarks>
	/// Implementation details:
	/// - Logs the filters being used for debugging purposes (as comma-separated key:value pairs)
	/// - Delegates to IExperimentManager.GetFlightsAsync for actual experiment allocation
	/// - Returns the raw response from the experiment manager without modification
	/// </remarks>
	public async Task<IDictionary<string, object>> GetExperimentalFeaturesAsync(IDictionary<string, object> filters, CancellationToken cancellationToken)
	{
		using Activity? activity = activitySource
			.StartActivity(FeatureManagementActivityNames.FeatureGatesService.GetExperimentalFeaturesAsync)?
			.MarkAsSystemError();

		IEnumerable<string> filtersUsed = filters.Select(item => $"{item.Key}:{item.Value}");
		logger.LogInformation(Tag.Create(), $"{nameof(FeatureGatesService)}.{nameof(GetExperimentalFeaturesAsync)} allocating experiment with the following filters: '{{Filters}}'.", string.Join(',', filtersUsed));

		IDictionary<string, object> response = await experimentManager.GetFlightsAsync(filters, cancellationToken);
		activity?.MarkAsSuccess();
		return response;
	}

	/// <inheritdoc />
	/// <remarks>
	/// Implementation details:
	/// - Calls GetExperimentalFeaturesAsync to retrieve all experimental features
	/// - Attempts to find the specified feature gate in the results
	/// - Parses the value to determine treatment allocation:
	///   * Missing key: Returns InTreatment=false
	///   * Empty/whitespace value: Returns InTreatment=false
	///   * Boolean true/false: Returns corresponding InTreatment value
	///   * Non-Boolean string: Returns InTreatment=true with the string as Value
	///
	/// The non-Boolean string handling allows experiments to return configuration values (e.g., "variant_a",
	/// "high_performance") rather than just on/off states.
	/// </remarks>
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

		// We received a feature-gate value for the experiment, so the customer is allocated. The value could not be
		// parsed so assume that the values have alternative meanings. For this to work, experiments should be setup to
		// return "false" for the control feature gate.
		return new(true, featureGateValue);
	}

	/// <inheritdoc/>
	/// <remarks>
	/// Implementation details:
	/// - Directly delegates to IExtendedFeatureManager.IsEnabledAsync
	/// - Does not consider experiments or overrides (pure configuration check)
	///
	/// This method is typically used as a fallback when experiment allocation fails or returns no result, providing a
	/// global on/off switch for features.
	/// </remarks>
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
	/// <remarks>
	/// Implementation details and evaluation order:
	///
	/// 1. Query-String Override Check (Highest Priority):
	///    - Checks IExtendedFeatureManager.GetOverride for explicit overrides
	///    - If present, immediately returns the override value
	///    - Sets Activity metadata to "FromOverride_{featureGate}"
	///
	/// 2. Experiment Check:
	///    - Calls GetExperimentalFeaturesAsync to get all experimental features
	///    - If feature is present with a non-empty value:
	///      * Attempts to parse as Boolean
	///      * Non-Boolean values return true (indicates treatment allocation)
	///      * Sets Activity metadata to "FromExperiment_{featureGate}"
	///
	/// 3. Static Configuration Fallback:
	///    - If feature not in experiments or has empty value
	///    - Checks IExtendedFeatureManager.IsEnabledAsync
	///    - Sets Activity metadata to "FromFeatureManager_{featureGate}"
	/// </remarks>
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
			// We received a feature-gate value for the experiment, so the customer is allocated. The value could not be
			// parsed so assume that the values have alternative meanings. For this to work, experiments should be setup
			// to return "false" for the control feature gate.
			return true;
		}

		return variable;
	}

	/// <summary>
	/// Updates the feature map with the override values from enabled or disabled feature lists.
	/// </summary>
	/// <remarks>
	/// This helper method applies bulk overrides to the feature-gates dictionary. It handles the "FE_" prefix stripping
	/// for frontend features and directly sets the override value for each feature in the list.
	///
	/// Unlike the main feature evaluation, this method uses direct assignment (not
	/// <see cref="Dictionary{TKey, TValue}.TryAdd(TKey, TValue)"/>), so override values will replace any previously set
	/// values in the dictionary.
	/// </remarks>
	/// <param name="featureGates">The feature-gates dictionary to update.</param>
	/// <param name="features">The list of feature names to override.</param>
	/// <param name="overrideValue">The value to set for all features in the list (<c>true</c> for enabled, <c>false</c> for disabled).</param>
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
