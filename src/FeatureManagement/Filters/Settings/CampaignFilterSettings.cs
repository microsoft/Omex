// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.Filters.Settings;

using System.Collections.Generic;

/// <summary>
/// The configuration settings for the campaign filter.
/// </summary>
public sealed class CampaignFilterSettings
{
	/// <summary>
	/// Gets or sets the list of campaigns for which the feature should be disabled.
	/// </summary>
	public List<string> Disabled { get; set; } = [];

	/// <summary>
	/// Gets or sets the list of campaigns for which the feature should be enabled.
	/// </summary>
	public List<string> Enabled { get; set; } = [];
}
