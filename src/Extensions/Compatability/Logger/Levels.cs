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
		public static LogLevel Error { get; } = LogLevel.Error;


		/// <summary>
		/// Info level
		/// </summary>
		public static LogLevel Info { get; } = LogLevel.Information;


		/// <summary>
		/// Spam level
		/// </summary>
		public static LogLevel Spam { get; } = LogLevel.Trace;


		/// <summary>
		/// Verbose level
		/// </summary>
		public static LogLevel Verbose { get; } = LogLevel.Debug;


		/// <summary>
		/// Warning level
		/// </summary>
		public static LogLevel Warning { get; } = LogLevel.Warning;
	}
}
