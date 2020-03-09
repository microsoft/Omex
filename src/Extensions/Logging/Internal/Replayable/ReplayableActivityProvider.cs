// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.TimedScopes;

namespace Microsoft.Omex.Extensions.Logging.Replayable
{
	/// <summary>
	/// Depending on <see cref="OmexLoggingOptions" /> class will create regular <see cref="Activity"/>
	/// or <see cref="ReplayableActivity" /> that is able to aggregate log messages for replay
	/// </summary>
	internal class ReplayableActivityProvider : IActivityProvider
	{
		public ReplayableActivityProvider(IOptions<OmexLoggingOptions> options) =>
			m_replayLogsInCaseOfError = options.Value.ReplayLogsInCaseOfError;


		public Activity Create(TimedScopeDefinition definition) =>
			m_replayLogsInCaseOfError ? new ReplayableActivity(definition.Name) : new Activity(definition.Name);


		private readonly bool m_replayLogsInCaseOfError;
	}
}
