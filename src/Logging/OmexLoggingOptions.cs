// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Logging
{
	/// <summary>
	/// Options for Omex logger
	/// </summary>
	internal class OmexLoggingOptions
	{
		/// <summary>
		/// Default configuration section to bind these options.
		/// </summary>
		public const string DefaultConfigurationSection = "OmexLogging";

		/// <summary>
		/// If false, OmexLoggerProvider returns NullLogger instead of OmexLogger.
		/// </summary>
		public bool Enabled { get; set; } = true;

		/// <summary>
		/// Should logs wrapped by the Activity be stored and replayed at a higher severity, in the event of an error.
		/// Setting this to true will impact the performance of logging
		/// </summary>
		public bool ReplayLogsInCaseOfError { get; set; } = false;

		/// <summary>
		/// Enabling this option will add CorrelationId guid to activity that will increase its size
		/// </summary>
		public bool AddObsoleteCorrelationToActivity { get; set; } = true;

		/// <summary>
		/// Maximum number of events that activity can store for replay
		/// </summary>
		public uint MaxReplayedEventsPerActivity { get; set; } = 1000;
	}
}
