// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Logging.TimedScopes
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
				default:
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
