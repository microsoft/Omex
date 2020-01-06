// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.System.Logging;
using Microsoft.Omex.System.Validation;

namespace Microsoft.Omex.System.TimedScopes.ReplayEventLogging
{
	/// <summary>
	/// Configures event replaying when a timed scope ends
	/// </summary>
	public class ReplayEventConfigurator : IReplayEventConfigurator
	{
		private IReplayEventDisabledTimedScopes DisabledTimedScopes { get; }


		private Correlation Correlation { get; }


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="disabledTimedScopes">Disabled timed scopes</param>
		/// <param name="correlation">Correlation</param>
		public ReplayEventConfigurator(IReplayEventDisabledTimedScopes disabledTimedScopes, Correlation correlation)
		{
			Code.ExpectsArgument(disabledTimedScopes, nameof(disabledTimedScopes), TaggingUtilities.ReserveTag(0x23817714 /* tag_96x2u */));
			Code.ExpectsArgument(correlation, nameof(correlation), TaggingUtilities.ReserveTag(0x23817715 /* tag_96x2v */));

			DisabledTimedScopes = disabledTimedScopes;
			Correlation = correlation;
		}


		/// <summary>
		/// Configure event replaying when a timed scope ends
		/// </summary>
		/// <param name="scope"></param>
		public void ConfigureReplayEventsOnScopeEnd(TimedScope scope)
		{
			CorrelationData currentCorrelation = Correlation.CurrentCorrelation;

			if (scope.IsSuccessful ?? false)
			{
				// assumption is that if any lower level scopes fail that should bubble up to the parent scope; if replay is enabled a previous scope has failed so
				// log some telemetry to help us understand these mixed scenarios better / identify error handling bugs
				if (currentCorrelation != null && currentCorrelation.ShouldReplayUls)
				{
					// ASSERTTAG_IGNORE_START
					ULSLogging.LogTraceTag(0, Categories.TimingGeneral, Levels.Warning,
					"Scope '{0}' succeeded even though a previous scope on this correlation failed.", scope.Name);
					// ASSERTTAG_IGNORE_FINISH
				}
			}
			else
			{
				// flip the replay switch on Scope failure for scenarios where its useful to get a verbose ULS trace in production
				if (currentCorrelation != null &&
					scope.Result.ShouldReplayEvents() &&
					!scope.IsTransaction &&
					!scope.ScopeDefinition.OnDemand &&
					!scope.DisableVerboseUlsCapture &&
					!DisabledTimedScopes.IsDisabled(scope.ScopeDefinition))
				{
					currentCorrelation.ShouldReplayUls = true;
					currentCorrelation.ReplayPreviouslyCachedUlsEvents();
				}
			}
		}
	}
}
