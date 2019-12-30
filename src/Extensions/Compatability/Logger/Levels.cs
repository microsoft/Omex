// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Compatability.Logger
{
	/// <summary>
	/// List of all available levels for logging
	/// </summary>
	public static class Levels
	{
		/// <summary>
		/// Error level
		/// </summary>
		public static readonly Level Error = new Level(LogLevel.Error);


		/// <summary>
		/// Info level
		/// </summary>
		public static readonly Level Info = new Level(LogLevel.Info);


		/// <summary>
		/// Spam level
		/// </summary>
		public static readonly Level Spam = new Level(LogLevel.Spam);


		/// <summary>
		/// Verbose level
		/// </summary>
		public static readonly Level Verbose = new Level(LogLevel.Verbose);


		/// <summary>
		/// Warning level
		/// </summary>
		public static readonly Level Warning = new Level(LogLevel.Warning);


		/// <summary>
		/// Enum defining the log levels
		/// </summary>
		public enum LogLevel
		{
			/// <summary> The Error log level </summary>
			Error = 0,
			/// <summary> The Info log level </summary>
			Info = 1,
			/// <summary> The Spam log level </summary>
			Spam = 2,
			/// <summary> The Verbose log level </summary>
			Verbose = 3,
			/// <summary> The Warning log level </summary>
			Warning = 4,
		}
	}
}
