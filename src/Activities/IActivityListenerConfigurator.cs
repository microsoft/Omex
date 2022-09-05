// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;

namespace Microsoft.Omex.Extensions.Activities
{
	/// <summary>
	/// Configures behavior of ActivityListener to decide when activity should be created based on additional information about them
	/// </summary>
	public interface IActivityListenerConfigurator
	{
		/// <summary>
		/// Decides if activity object events that were created using the activity source object should be listened or not
		/// </summary>
		bool ShouldListenTo(ActivitySource activity);

		/// <summary>
		/// Decides if creating System.Diagnostics.Activity objects with a specific data state is allowed
		/// Used when parentId specified
		/// </summary>
		ActivitySamplingResult SampleUsingParentId(ref ActivityCreationOptions<string> options);

		/// <summary>
		/// Decides if creating System.Diagnostics.Activity objects with a specific data state is allowed
		/// </summary>
		ActivitySamplingResult Sample(ref ActivityCreationOptions<ActivityContext> options);
	}
}
