// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.TimedScopes;

namespace Microsoft.Omex.Extensions.Logging.Replayable
{
	internal class ReplayableActivityProvider : IActivityProvider
	{
		public ReplayableActivityProvider(IOptions<OmexLoggingOptions> options) =>
			m_replayLogsInCaseOfError = options.Value.ReplayLogsInCaseOfError;


		public Activity Create(string operationName) =>
			m_replayLogsInCaseOfError ? new ReplayableActivity(operationName) : new Activity(operationName);


		private readonly bool m_replayLogsInCaseOfError;
	}
}
