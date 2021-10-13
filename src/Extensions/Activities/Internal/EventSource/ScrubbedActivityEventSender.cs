// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions.ExecutionContext;

namespace Microsoft.Omex.Extensions.Activities
{
	internal sealed class ScrubbedActivityEventSender : IActivityEventSender
	{
		private readonly ActivityEventSender m_activityEventSender;

		public ScrubbedActivityEventSender(ActivityEventSource eventSource, IExecutionContext executionContext, ILogger<IActivityEventSender> logger) =>
			m_activityEventSender = new ActivityEventSender(eventSource, executionContext, logger);

		public void SendActivityMetric(Activity activity) =>
			m_activityEventSender.SendActivityMetric(activity, true);
	}
}
