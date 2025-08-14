// Copyright (C) Microsoft Corporation. All rights reserved.

namespace Microsoft.Omex.Extensions.FeatureManagement;

/// <summary>
/// The feature override settings.
/// </summary>
public sealed class FeatureOverrideSettings
{
	/// <summary>
	/// Gets or sets the set of features that are always off regardless of their filter evaluation.
	/// </summary>
	public string[] Disabled { get; set; } = [];

	/// <summary>
	/// Gets or sets the set of features that are always on regardless of their filter evaluation.
	/// </summary>
	public string[] Enabled { get; set; } = [];

	/// <summary>
	/// Gets or sets the set of features that are off or disabled with other settings, and allow them to evaluate the applied filters.
	/// </summary>
	public string[] Toggled { get; set; } = [];
}
