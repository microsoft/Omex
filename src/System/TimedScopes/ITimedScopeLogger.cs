// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Interface for logging timed scopes
	/// </summary>
	public interface ITimedScopeLogger
	{
		/// <summary>
		/// Logs the scope start
		/// </summary>
		/// <param name="scope">Scope to log</param>
		void LogScopeStart(TimedScope scope);


		/// <summary>
		/// Logs the scope end
		/// </summary>
		/// <param name="scope">Scope to log</param>
		/// <param name="data">Correlation data</param>
		void LogScopeEnd(TimedScope scope, CorrelationData data);
	}
}
