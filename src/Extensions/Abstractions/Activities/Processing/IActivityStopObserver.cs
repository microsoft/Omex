// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;

namespace Microsoft.Omex.Extensions.Abstractions.Activities.Processing
{
	/// <summary>
	/// Interface to monitor activity stop
	/// </summary>
	public interface IActivityStopObserver
	{
		/// <summary>
		/// Method will be called after activity stop
		/// </summary>
		void OnStop(Activity activity, object? payload = null);
	}
}
