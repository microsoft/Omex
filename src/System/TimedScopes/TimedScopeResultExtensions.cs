// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Omex.System.Logging;

namespace Microsoft.Omex.System.TimedScopes
{
	/// <summary>
	/// Extensions for TimedScopeResult enum
	/// </summary>
	public static class TimedScopeResultExtensions
	{
		/// <summary>
		/// Decides whether the result is success
		/// </summary>
		/// <param name="result">The result</param>
		/// <returns>Success flag (null if we don't know)</returns>
		public static bool? IsSuccessful(this TimedScopeResult result)
		{
			switch (result)
			{
				case default(TimedScopeResult):
					return null;

				case TimedScopeResult.Success:
					return true;

				case TimedScopeResult.ExpectedError:
				case TimedScopeResult.SystemError:
					return false;

				default:
					ULSLogging.LogTraceTag(0x23850347 /* tag_97qnh */, Categories.TimingGeneral, Levels.Error, "IsSuccessful status unknown for timed scope result {0}", result);
					return false;
			}
		}


		/// <summary>
		/// Decides whether we should replay events for scopes with given result
		/// </summary>
		/// <param name="result">The result</param>
		/// <returns>true if we should replay events for this result; false otherwise</returns>
		public static bool ShouldReplayEvents(this TimedScopeResult result)
		{
			switch (result)
			{
				case TimedScopeResult.SystemError:
					return true;

				default:
					return false;
			}
		}
	}	
}