// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;

namespace Microsoft.Omex.Extensions.Compatibility.Logger
{
	/// <summary>
	/// List of all available levels for logging
	/// </summary>
	public static class Levels
	{
		/// <summary>
		/// Error level
		/// </summary>
		public static LogLevel Error => LogLevel.Error;


		/// <summary>
		/// Info level
		/// </summary>
		public static LogLevel Info => LogLevel.Information;


		/// <summary>
		/// Spam level
		/// </summary>
		public static LogLevel Spam => LogLevel.Trace;


		/// <summary>
		/// Verbose level
		/// </summary>
		public static LogLevel Verbose => LogLevel.Debug;


		/// <summary>
		/// Warning level
		/// </summary>
		public static LogLevel Warning => LogLevel.Warning;
	}
}
