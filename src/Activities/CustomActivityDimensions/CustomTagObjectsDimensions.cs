// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;

namespace Microsoft.Omex.Extensions.Activities
{
	/// <inheritdoc/>
	public class CustomTagObjectsDimensions : ICustomTagObjectsDimensions
	{
		/// <summary>
		/// Default constructor which has <seealso cref="ActivityTagKeys.Result"/>, <seealso cref="ActivityTagKeys.SubType"/>, <seealso cref="ActivityTagKeys.Metadata"/>, <seealso cref="ActivityTagKeys.Dependent"/>
		/// </summary>
		public CustomTagObjectsDimensions() => CustomDimensions = new HashSet<string>() { ActivityTagKeys.Result, ActivityTagKeys.SubType, ActivityTagKeys.Metadata, ActivityTagKeys.Dependent };

		/// <summary>
		/// Override the default constructor to specify custom dimension set.
		/// </summary>
		/// <param name="overrideDefaultCustomDimensions">Set of custom dimensions which are added to metric point</param>
		public CustomTagObjectsDimensions(HashSet<string> overrideDefaultCustomDimensions) => CustomDimensions = overrideDefaultCustomDimensions;

		/// <inheritdoc/>
		public HashSet<string> CustomDimensions { get; }
	}
}
