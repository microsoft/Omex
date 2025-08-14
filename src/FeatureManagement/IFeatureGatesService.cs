// Copyright (C) Microsoft Corporation. All rights reserved.

namespace Microsoft.Omex.Extensions.FeatureManagement;

using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// The service managing the feature gates.
/// </summary>
/// <remarks>This service is responsible for managing feature gates (feature flags) and experimentation
/// within the app. It provides APIs to query which features are enabled, which are blocked, and which experimental
/// treatments apply to a customer, supporting both static feature flags and dynamic experimentation
/// scenarios.</remarks>
public interface IFeatureGatesService
{
	/// <summary>
	/// Gets the list of features which are explicitly allowed for this request as a <see cref="string"/>.
	/// </summary>
	string RequestedFeatures { get; }

	/// <summary>
	/// Gets the list of features which are explicitly blocked for this request as a <see cref="string"/>.
	/// </summary>
	string BlockedFeatures { get; }

	/// <summary>
	/// Gets all the feature gates and their values.
	/// </summary>
	/// <returns>A dictionary mapping the feature gate names to their values.</returns>
	Task<IDictionary<string, object>> GetFeatureGatesAsync();

	/// <summary>
	/// Checks if the feature gate is active.
	/// </summary>
	/// <param name="featureGate">The feature gate.</param>
	/// <returns><c>true</c> if the feature gate is active.</returns>
	Task<bool> IsFeatureGateApplicableAsync(string featureGate);
}
