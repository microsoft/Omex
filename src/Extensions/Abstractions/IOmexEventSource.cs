// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.Logging
{
	/// <summary>ETW source for log aggregation system</summary>
	public interface IOmexEventSource
	{
		/// <summary>
		/// Create ETW event that would be reported to log aggregation system
		/// </summary>
		/// <param name="activityId">Activity Id</param>
		/// <param name="logInformation">Log Information</param>
		void ReplayEvent(string activityId, LogMessageInformation logInformation);
	}
}
