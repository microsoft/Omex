// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.Filters.Filter.Settings
{
	/// <summary>
	/// The configuration settings for the parent filter.
	/// </summary>
	internal sealed class ParentFilterSettings
	{
		/// <summary>
		/// Gets or sets the name of the parent feature.
		/// </summary>
		public string Feature { get; set; } = string.Empty;
	}
}
