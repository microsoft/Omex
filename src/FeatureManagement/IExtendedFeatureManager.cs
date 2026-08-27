// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement;

using Microsoft.FeatureManagement;

/// <summary>
/// The extended feature manager provides support for server-controlled overriding or toggling of features.
/// </summary>
/// <remarks>Request-driven feature overrides are not supported. Use <see cref="FeatureOverrideSettings"/> for
/// server-controlled overrides.</remarks>
public interface IExtendedFeatureManager : IFeatureManager
{
	/// <summary>
	/// Gets the override value for a feature or <c>null</c> if none exists.
	/// </summary>
	/// <param name="feature">The name of the feature flag to check.</param>
	/// <returns>The override value for the feature or <c>null</c> if none exists.</returns>
	bool? GetOverride(string feature);
}
