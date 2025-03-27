// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Omex.Extensions.Activities
{
	/// <summary>
	/// Define custom metric dimensions which are extracted from Activity TagObjects  by <see cref="ActivityMetricsSender.SendActivityMetric"/>
	/// Make sure that the value of any custom dimensions are not random values such as ID, timestamp because it will lead to metric explosion.
	/// </summary>
	public interface ICustomTagObjectsDimensions
	{
		/// <summary>
		/// Custom dimension set
		/// </summary>
		HashSet<string> CustomDimensions { get; }
	}
}
