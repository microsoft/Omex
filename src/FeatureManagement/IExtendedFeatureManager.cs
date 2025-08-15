// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement;

using Microsoft.FeatureManagement;

/// <summary>
/// The extended feature manager provides support for dynamic overriding or toggling of features.
/// </summary>
/// <remarks>This extends feature flag management by supporting dynamic, per-request feature overrides using
/// query-string parameters and configuration settings. This is particularly useful for scenarios where you want to
/// enable or disable features for specific requests, such as for testing, gradual rollouts, or customer-specific
/// toggling.</remarks>
public interface IExtendedFeatureManager : IFeatureManager
{
	/// <summary>
	/// Gets the list of features which are explicitly disabled for this request as a <see cref="string"/>.
	/// </summary>
	string DisabledFeatures { get; }

	/// <summary>
	/// Gets the list of features which are explicitly disabled for this request as a <see cref="string"/> array.
	/// </summary>
	string[] DisabledFeaturesList { get; }

	/// <summary>
	/// Gets the list of features which are explicitly enabled for this request as a <see cref="string"/>.
	/// </summary>
	string EnabledFeatures { get; }

	/// <summary>
	/// Gets the list of features which are explicitly enabled for this request as a <see cref="string"/> array.
	/// </summary>
	string[] EnabledFeaturesList { get; }

	/// <summary>
	/// Gets the override value for a feature or <c>null</c> if none exists.
	/// </summary>
	/// <param name="feature">The name of the feature flag to check.</param>
	/// <returns>The override value for the feature or <c>null</c> if none exists.</returns>
	bool? GetOverride(string feature);
}
