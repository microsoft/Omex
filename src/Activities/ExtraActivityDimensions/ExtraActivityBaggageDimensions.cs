// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Omex.Extensions.Activities
{
	/// <inheritdoc/>
	public class ExtraActivityBaggageDimensions : IExtraActivityBaggageDimensions
	{
		/// <summary>
		/// Default constructor which has no extra baggage dimensions
		/// </summary>
		public ExtraActivityBaggageDimensions() => ExtraDimensions = new HashSet<string>();

		/// <summary>
		/// Override the default constructor to specify extra dimension set.
		/// </summary>
		/// <param name="overrideDefaultExtraDimensions"></param>
		public ExtraActivityBaggageDimensions(HashSet<string> overrideDefaultExtraDimensions) => ExtraDimensions = overrideDefaultExtraDimensions;

		/// <inheritdoc/>
		public HashSet<string> ExtraDimensions { get; }
	}
}
