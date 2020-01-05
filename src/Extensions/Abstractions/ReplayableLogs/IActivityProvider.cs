// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;

namespace Microsoft.Omex.Extensions.Abstractions
{
	/// <summary>Provides activities</summary>
	public interface IActivityProvider
	{
		/// <summary>
		/// Create activity instance
		/// </summary>
		/// <param name="operationName">The name of the operation</param>
		/// <param name="replayLogsInCaseOfError">Should activity collect log events to replay them in case of an error</param>
		/// <returns></returns>
		Activity Create(string operationName, bool replayLogsInCaseOfError = true);
	}
}
