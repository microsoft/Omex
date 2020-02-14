// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.Omex.Extensions.Abstractions;

namespace Microsoft.Omex.Extensions.Logging.Replayable
{
	internal class ReplayibleActivityProvider : IActivityProvider
	{
		public Activity Create(string operationName, bool replayLogsInCaseOfError) =>
			replayLogsInCaseOfError ? new ReplayableActivity(operationName) : new Activity(operationName);
	}
}
