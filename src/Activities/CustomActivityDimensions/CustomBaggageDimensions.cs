// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Omex.Extensions.Activities
{
	/// <inheritdoc/>
	public class CustomBaggageDimensions : ICustomBaggageDimensions
	{
		/// <summary>
		/// Default constructor which has no custom baggage dimensions
		/// </summary>
		public CustomBaggageDimensions() => CustomDimensions = new HashSet<string>();

		/// <summary>
		/// Override the default constructor to specify custom dimension set.
		/// </summary>
		/// <param name="overrideDefaultCustomDimensions">Set of custom dimensions which are added to metric point</param>
		public CustomBaggageDimensions(HashSet<string> overrideDefaultCustomDimensions) => CustomDimensions = overrideDefaultCustomDimensions;

		/// <inheritdoc/>
		public HashSet<string> CustomDimensions { get; }
	}
}
