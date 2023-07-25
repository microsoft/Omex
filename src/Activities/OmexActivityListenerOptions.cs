// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;

namespace Microsoft.Omex.Extensions.Activities
{
	/// <summary>
	/// Configuration options for activity creation
	/// </summary>
	/// <remarks>
	///	Applied only when DefaultActivityListenerCongigurator used
	/// </remarks>
	public sealed class OmexActivityListenerOptions
	{
		/// <summary>
		/// Disable <see cref="ActivityEventSender"/> so Activity metric is only sent via <see cref="ActivityMetricsSender"/>
		/// </summary>
		public bool DisableEventSender { get; set; } = false;
		/// <summary>
		/// Decides if activity object events that were created using the activity source object should be listened or not
		/// </summary>
		public bool ShouldListenTo { get; set; } = true;

		/// <summary>
		/// Decides if creating System.Diagnostics.Activity objects with a specific data state is allowed
		/// Used when parentId specified
		/// </summary>
		public ActivitySamplingResult SampleUsingParentId { get; set; } = ActivitySamplingResult.AllDataAndRecorded;

		/// <summary>
		/// Decides if creating System.Diagnostics.Activity objects with a specific data state is allowed
		/// </summary>
		public ActivitySamplingResult Sample { get; set; } = ActivitySamplingResult.AllDataAndRecorded;
	}
}
