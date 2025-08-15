// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.Filters.Settings;

using System.Collections.Generic;

/// <summary>
/// The configuration settings for the environment filter.
/// </summary>
public sealed class EnvironmentFilterSettings
{
	/// <summary>
	/// Gets or sets the list of environments for which the feature should be enabled.
	/// </summary>
	public List<string> Environments { get; set; } = [];
}
