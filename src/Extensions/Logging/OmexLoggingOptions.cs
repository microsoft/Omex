// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.TimedScopes
{
	/// <summary>
	/// Options for Omex logger
	/// </summary>
	public class OmexLoggingOptions
	{
		/// <summary>
		/// Should log inside TimedScope be stored and replaed wiht higher severity in case of error.
		/// This settig could impact performance of logging.
		/// </summary>
		public bool ReplayLogsInCaseOfError { get; set; } = false;
	}
}
