// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Abstractions
{
	/// <summary>Interface to agregate and replace logs in certain scope</summary>
	public interface IReplayableLogScope
	{
		/// <summary>
		/// Add log event to enable it's replay in case of failure
		/// </summary>
		/// <param name="logMessage">log message informations</param>
		void AddLogEvent(LogMessageInformation logMessage);
	}
}
