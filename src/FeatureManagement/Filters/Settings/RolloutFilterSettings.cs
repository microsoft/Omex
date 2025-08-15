// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.Filters.Settings;

/// <summary>
/// The configuration settings for the rollout feature.
/// </summary>
public sealed class RolloutFilterSettings
{
	/// <summary>
	/// Gets or sets the rollout percentage for the feature.
	/// </summary>
	public int ExposurePercentage { get; set; }
}
