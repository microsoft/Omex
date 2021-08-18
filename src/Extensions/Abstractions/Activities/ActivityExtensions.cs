// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;

namespace Microsoft.Omex.Extensions.Abstractions.Activities
{
	/// <summary>
	/// Extensions for Activity
	/// </summary>
	public static class ActivityExtensions
	{
		/// <summary>
		/// Transforms RootId into Guid that might be used in places where old Correlation Id needed
		/// </summary>
		/// <remarks>RootId could be converted only for started Activity when W3C id format used, all other cases will return null</remarks>
		public static Guid? GetRootIdAsGuid(this Activity activity) =>
			Guid.TryParse(activity.RootId, out Guid result)
				? result
				: (Guid?)null;

		/// <summary>
		/// Get user hash from activity
		/// </summary>
		public static string GetUserHash(this Activity activity) =>
			activity.GetBaggageItem(UserHashKey) ?? string.Empty;

		/// <summary>
		/// Returns true if activity is transaction
		/// </summary>
		public static bool IsHealthCheck(this Activity activity) =>
			string.Equals(
				activity.GetBaggageItem(HealthCheckMarkerKey),
				HealthCheckMarkerValue,
				StringComparison.OrdinalIgnoreCase);

		/// <summary>
		/// Returns true if activity is marked as Performance test
		/// </summary>
		/// <param name="activity"></param>
		/// <returns>True if the activity is marked as a Performance test, false otherwise</returns>
		public static bool IsPerformanceTest(this Activity activity) =>
			!string.IsNullOrEmpty(activity.GetBaggageItem(PerformanceMarkerKey));

		/// <summary>
		/// Returns the value of the Performance key if it exists
		/// </summary>
		/// <param name="activity"></param>
		/// <returns>The value passed for PerformanceTestMarker</returns>
		public static string PerformanceValue(this Activity activity)
		{
			return activity.GetBaggageItem(PerformanceMarkerKey) ?? string.Empty;
		}

		/// <summary>
		/// Set user hash for the activity
		/// </summary>
		/// <remarks>This property would be transfered to child activity and via web requests</remarks>
		public static Activity SetUserHash(this Activity activity, string userHash) =>
			activity.SetBaggage(UserHashKey, userHash);

		/// <summary>
		/// Mark as health check activity
		/// </summary>
		/// <remarks>This property would be transfered to child activity and via web requests</remarks>
		public static Activity MarkAsHealthCheck(this Activity activity) =>
			activity.IsHealthCheck()
				? activity
				: activity.SetBaggage(HealthCheckMarkerKey, HealthCheckMarkerValue);

		/// <summary>
		/// Set result
		/// </summary>
		/// <remarks>This property won't be transferred to child activity or via web requests</remarks>
		public static Activity SetResult(this Activity activity, ActivityResult result) =>
			activity.SetTag(ActivityTagKeys.Result, ActivityResultStrings.ResultToString(result));

		/// <summary>
		/// Set activity result to Success
		/// </summary>
		/// <remarks>This property won't be transferred to child activity or via web requests</remarks>
		public static Activity MarkAsSuccess(this Activity activity) => activity.SetResult(ActivityResult.Success);

		/// <summary>
		/// Set activity result to SystemError
		/// </summary>
		/// <remarks>This property won't be transferred to child activity or via web requests</remarks>
		public static Activity MarkAsSystemError(this Activity activity) => activity.SetResult(ActivityResult.SystemError);

		/// <summary>
		/// Set activity result to ExpectedError
		/// </summary>
		/// <remarks>This property won't be transferred to child activity or via web requests</remarks>
		public static Activity MarkAsExpectedError(this Activity activity) => activity.SetResult(ActivityResult.ExpectedError);

		/// <summary>
		/// Set sub type
		/// </summary>
		/// <remarks>This property won't be transfered to child activity or via web requests</remarks>
		public static Activity SetSubType(this Activity activity, string subType) =>
			activity.SetTag(ActivityTagKeys.SubType, subType);

		/// <summary>
		/// Set metadata
		/// </summary>
		/// <remarks>This property won't be transfered to child activity or via web requests</remarks>
		public static Activity SetMetadata(this Activity activity, string metadata) =>
			activity.SetTag(ActivityTagKeys.Metadata, metadata);

		/// <summary>
		/// Get correlation guid that is used by old Omex services
		/// </summary>
		[Obsolete(CorrelationIdObsoleteMessage, false)]
		public static Guid? GetObsoleteCorrelationId(this Activity activity) =>
			Guid.TryParse(activity.GetBaggageItem(ObsoleteCorrelationId), out Guid correlation)
				? correlation
				: (Guid?)null;

		/// <summary>
		/// Set correlation guid that is used by old Omex services
		/// </summary>
		/// <remarks>This property would be transfered to child activity and via web requests</remarks>
		[Obsolete(CorrelationIdObsoleteMessage, false)]
		public static Activity SetObsoleteCorrelationId(this Activity activity, Guid correlation) =>
			activity.SetBaggage(ObsoleteCorrelationId, correlation.ToString());

		/// <summary>
		/// Get transaction id that is used by old Omex services
		/// </summary>
		[Obsolete(TransactionIdObsoleteMessage, false)]
		public static uint? GetObsoleteTransactionId(this Activity activity) =>
			uint.TryParse(activity.GetBaggageItem(ObsoleteTransactionId), out uint transactionId)
				? transactionId
				: (uint?)null;

		/// <summary>
		/// Set transaction id that is used by old Omex services
		/// </summary>
		/// <remarks>This property would be transfered to child activity and via web requests</remarks>
		[Obsolete(TransactionIdObsoleteMessage, false)]
		public static Activity SetObsoleteTransactionId(this Activity activity, uint transactionId) =>
			activity.SetBaggage(ObsoleteTransactionId, transactionId.ToString(CultureInfo.InvariantCulture));

		private const string UserHashKey = "UserHash";
		private const string HealthCheckMarkerKey = "HealthCheckMarker";
		private const string HealthCheckMarkerValue = "true";
		private const string PerformanceMarkerKey = "PerformanceTestMarker";
		private const string ObsoleteCorrelationId = "ObsoleteCorrelationId";
		private const string ObsoleteTransactionId = "ObsoleteTransactionId";
		private const string CorrelationIdObsoleteMessage = "Please use Activity.Id or Activity.GetRootIdAsGuid() for new services instead";
		private const string TransactionIdObsoleteMessage = "Please use Activity.IsHealthCheck() for new services instead";
	}
}
