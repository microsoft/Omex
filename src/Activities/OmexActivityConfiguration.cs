// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Activities
{
	/// <summary>
	/// Configuration for Omex activities.
	/// </summary>
	public class OmexActivityConfiguration
	{
		/// <summary>
		/// The name of the activities histogram metric.
		/// </summary>
		public const string ActivitiesHistogramName = "Activities";

		/// <summary>
		/// The name of the HealtchCheck activities histogram metric.
		/// </summary>
		public const string HealthCheckActivitiesHistogramName = "HealthCheckActivities";

		/// <summary>
		/// The name of the Meter that is used to record activity metrics.
		/// </summary>
		public const string MeterName = "Microsoft.Omex.Activities";
	}
}
