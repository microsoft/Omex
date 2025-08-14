// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Omex.Extensions.FeatureManagement.Filters.Filter.Settings
{
	/// <summary>
	/// The configuration settings for the campaign filter.
	/// </summary>
	internal sealed class CampaignFilterSettings
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
}
