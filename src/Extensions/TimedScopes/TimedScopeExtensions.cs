// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Omex.Extensions.Logging.TimedScopes
{
	/// <summary>
	/// Extensions for TimedScopeResult enum
	/// </summary>
	public static class TimedScopeExtensions
	{
		/// <summary>
		/// Decides whether the result is success
		/// </summary>
		/// <param name="result">The result</param>
		/// <returns>Success flag (null if we don't know)</returns>
		public static bool? IsSuccessful(this TimedScopeResult result) =>
			result switch
			{
				default(TimedScopeResult) => null,
				TimedScopeResult.Success => true,
				_ => false,
			};


		/// <summary>
		/// Decides whether we should replay events for scopes with given result
		/// </summary>
		/// <param name="result">The result</param>
		/// <returns>true if we should replay events for this result; false otherwise</returns>
		public static bool ShouldReplayEvents(this TimedScopeResult result) =>
			result switch
			{
				TimedScopeResult.SystemError => true,
				_ => false,
			};
	}

	/// <summary>
	/// Extensions for Activity
	/// </summary>
	public static class ActivityExtensions
	{
		private const string UserHashKey = "UserHash";
		private const string TransactionMarkerKey = "TransactionMarkerKey";
		private const string TransactionMarkerValue = "true";


		/// <summary>Get user hash from activity</summary>
		public static string GetUserHash(this Activity activity) =>
			activity.GetBaggageItem(UserHashKey);


		/// <summary>Returns true if activity is transaction</summary>
		public static bool IsTransaction(this Activity activity) =>
			activity.GetBaggageItem(TransactionMarkerKey) == TransactionMarkerValue;


		/// <summary>Set user hash from activity</summary>
		public static void SetUserHash(this Activity activity, string userHash) =>
			activity.AddBaggage(UserHashKey, userHash);


		/// <summary>Mark activity as transaction</summary>
		public static void MarkAsTransaction(this Activity activity) =>
			activity.AddBaggage(TransactionMarkerKey, TransactionMarkerValue);
	}
}
