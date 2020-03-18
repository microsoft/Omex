// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;

namespace Microsoft.Omex.Extensions.TimedScopes
{
	internal sealed class ActivityStopObserver : IActivityStopObserver
	{
		private readonly ITimedScopeEventSender m_eventSender;

		public ActivityStopObserver(ITimedScopeEventSender timedScopeEventSender) =>
			m_eventSender = timedScopeEventSender;

		public void OnStop(Activity activity, object? payload) =>
			m_eventSender.LogActivityStop(activity);
	}
}
