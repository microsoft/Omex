// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;

namespace Microsoft.Omex.Extensions.Activities
{
	internal sealed class ActivityStopObserver : IActivityStopObserver
	{
		private readonly IActivitiesEventSender m_eventSender;

		public ActivityStopObserver(IActivitiesEventSender timedScopeEventSender) =>
			m_eventSender = timedScopeEventSender;

		public void OnStop(Activity activity, object? payload) =>
			m_eventSender.LogActivityStop(activity);
	}
}
