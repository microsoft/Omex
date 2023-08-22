// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Omex.Extensions.Activities
{
	/// <summary>
	/// Define extra metric dimensions which are extracted from Activity Baggage  by <see cref="ActivityMetricsSender.SendActivityMetric"/>
	/// Make sure that the value of any extra dimensions are not random values such as ID, timestamp because it will lead to metric explosion.
	/// </summary>
	public interface IExtraActivityBaggageDimensions
	{
		/// <summary>
		/// Extra dimension set
		/// </summary>
		HashSet<string> ExtraDimensions { get; }
	}
}
