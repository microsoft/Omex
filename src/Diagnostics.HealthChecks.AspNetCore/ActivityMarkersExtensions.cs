// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Diagnostics;

namespace Microsoft.Omex.Extensions.Diagnostics.HealthChecks.AspNetCore;

/// <summary>
/// A set of extensions for Activity markers.
/// </summary>
public static class ActivityMarkersExtensions
{
	/// <summary>
	/// The key used to identify the Short Circuited Activity.
	/// </summary>
	public const string LivenessCheckActivityKey = "LivenessCheckMarker";

	/// <summary>
	/// The value used to identify the Short Circuited Activity.
	/// </summary>
	public const string LivenessCheckActivityValue = "true";

	/// <summary>
	/// Determines whether the activity has been marked as belonging to a health check whose call should be
	/// short-circuited.
	/// </summary>
	/// <param name="activity">The Activity.</param>
	/// <returns><c>True</c> if the activity's health check should be short-circuited, <c>False</c> otherwise.</returns>
	public static bool IsLivenessCheck(this Activity activity) =>
		string.Equals(activity.GetBaggageItem(LivenessCheckActivityKey), LivenessCheckActivityValue, StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Marks the Activity as belonging to a liveness health check whose call should be short-circuited.
	/// </summary>
	/// <param name="activity">The activity.</param>
	/// <returns>The marked activity.</returns>
	public static Activity MarkAsLivenessCheck(this Activity activity)
	{
		if (!activity.IsLivenessCheck())
		{
			return activity.AddBaggage(LivenessCheckActivityKey, LivenessCheckActivityValue);
		}

		return activity;
	}
}
