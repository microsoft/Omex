// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Omex.Extensions.FeatureManagement.Filters.Settings
{
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
}
