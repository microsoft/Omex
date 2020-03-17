// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Logging
{
	/// <summary>
	/// Options for Omex logger
	/// </summary>
	public class OmexLoggingOptions
	{
		/// <summary>
		/// Should logs wrapped by the TimedScope be stored and replayed at a higher severity, in the event of an error.
		/// Setting this to true will impact the performance of logging
		/// </summary>
		public bool ReplayLogsInCaseOfError { get; set; } = false;


		/// <summary>
		/// Maximum number of events that activity can store for replay
		/// </summary>
		public uint MaxReplayedEventsPerActivity { get; set; } = 1000;
	}
}
