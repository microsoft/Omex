// Copyright (C) Microsoft Corporation. All rights reserved.

namespace Microsoft.Omex.Extensions.FeatureManagement.Filters.Filter.Settings;

/// <summary>
/// The configuration settings for the parent filter.
/// </summary>
public sealed class ParentFilterSettings
{
	/// <summary>
	/// Gets or sets the name of the parent feature.
	/// </summary>
	public string Feature { get; set; } = string.Empty;
}
