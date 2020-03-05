// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;

namespace Microsoft.Omex.Extensions.TimedScopes
{
	/// <summary>
	/// Extensions for Activity
	/// </summary>
	public static class ActivityExtensions
	{
		/// <summary>
		/// Get user hash from activity
		/// </summary>
		public static string GetUserHash(this Activity activity) =>
			activity.GetBaggageItem(UserHashKey);


		/// <summary>
		/// Returns true if activity is transaction
		/// </summary>
		public static bool IsTransaction(this Activity activity) =>
			string.Equals(
				activity.GetBaggageItem(TransactionMarkerKey),
				TransactionMarkerValue,
				StringComparison.OrdinalIgnoreCase);


		/// <summary>
		/// Set user hash from activity
		/// </summary>
		public static void SetUserHash(this Activity activity, string userHash) =>
			activity.AddBaggage(UserHashKey, userHash);


		/// <summary>
		/// Mark activity as transaction
		/// </summary>
		public static void MarkAsTransaction(this Activity activity) =>
			activity.AddBaggage(TransactionMarkerKey, TransactionMarkerValue);


		private const string UserHashKey = "UserHash";
		private const string TransactionMarkerKey = "TransactionMarkerKey";
		private const string TransactionMarkerValue = "true";
	}
}
