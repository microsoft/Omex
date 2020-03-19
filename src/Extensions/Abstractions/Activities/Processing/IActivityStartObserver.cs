// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;

namespace Microsoft.Omex.Extensions.Abstractions.Activities.Processing
{
	/// <summary>
	/// Interface to monitor activity start
	/// </summary>
	public interface IActivityStartObserver
	{
		/// <summary>
		/// Method will be called after activity start
		/// </summary>
		void OnStart(Activity activity, object? payload);
	}
}
