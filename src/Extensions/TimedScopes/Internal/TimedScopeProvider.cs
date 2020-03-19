// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.Extensions.Abstractions.Activities;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;

namespace Microsoft.Omex.Extensions.TimedScopes
{
	internal sealed class TimedScopeProvider : ITimedScopeProvider
	{
		public TimedScopeProvider(IActivityProvider activityProvider) =>
			m_activityProvider = activityProvider;

		public TimedScope CreateAndStart(TimedScopeDefinition name, TimedScopeResult result) =>
			Create(name, result).Start();

		public TimedScope Create(TimedScopeDefinition name, TimedScopeResult result) =>
			new TimedScope(m_activityProvider.Create(name), result);

		private readonly IActivityProvider m_activityProvider;
	}
}
