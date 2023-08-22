// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;

namespace Microsoft.Omex.Extensions.Activities
{
	/// <inheritdoc/>
	public class ExtraActivityTagObjectsDimensions : IExtraActivityTagObjectsDimensions
	{
		/// <summary>
		/// Default constructor which has <seealso cref="ActivityTagKeys.Result"/>, <seealso cref="ActivityTagKeys.SubType"/>, <seealso cref="ActivityTagKeys.Metadata"/>
		/// </summary>
		public ExtraActivityTagObjectsDimensions() => ExtraDimensions = new HashSet<string>() { ActivityTagKeys.Result, ActivityTagKeys.SubType, ActivityTagKeys.Metadata };

		/// <summary>
		/// Override the default constructor to specify extra dimension set.
		/// </summary>
		/// <param name="overrideDefaultExtraDimensions"></param>
		public ExtraActivityTagObjectsDimensions(HashSet<string> overrideDefaultExtraDimensions) => ExtraDimensions = overrideDefaultExtraDimensions;

		/// <inheritdoc/>
		public HashSet<string> ExtraDimensions { get; }
	}
}
