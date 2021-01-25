// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;

namespace Microsoft.Omex.Extensions.Activities
{
	/// <summary>
	/// Activity event source
	/// </summary>
	public interface IActivitiesEventSender
	{
		/// <summary>
		/// Log information about activity stop
		/// </summary>
		void LogActivityStop(Activity activity);
	}
}
