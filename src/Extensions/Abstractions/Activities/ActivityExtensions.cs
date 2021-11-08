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
				: null;

		/// <summary>
		/// Get user hash from activity
		/// </summary>
		public static string GetUserHash(this Activity activity) =>
			activity.GetBaggageItem(s_userHashKey) ?? string.Empty;

		/// <summary>
		/// Returns true if activity is transaction
		/// </summary>
		public static bool IsHealthCheck(this Activity activity) =>
			string.Equals(
				activity.GetBaggageItem(s_healthCheckMarkerKey),
				s_healthCheckMarkerValue,
				StringComparison.OrdinalIgnoreCase);

		/// <summary>
		/// Returns true if activity is marked as Performance test
		/// </summary>
		/// <param name="activity">Activity for this request</param>
		/// <returns>True if the activity is marked as a Performance test, false otherwise</returns>
		/// <remarks>Currently added to the BaggageItems (via Kestrel) using header value Correlation-Context: "PerformanceTestMarker=true"</remarks>
		public static bool IsPerformanceTest(this Activity activity) =>
			string.Equals(
				activity.GetBaggageItem(s_performanceMarkerKey),
				s_performanceMarkerValue,
				StringComparison.OrdinalIgnoreCase);

		/// <summary>
		/// Set user hash for the activity
		/// </summary>
		/// <remarks>This property would be transfered to child activity and via web requests</remarks>
		public static Activity SetUserHash(this Activity activity, string userHash) =>
			activity.SetBaggage(s_userHashKey, userHash);

		/// <summary>
		/// Mark as health check activity
		/// </summary>
		/// <remarks>This property would be transfered to child activity and via web requests</remarks>
		public static Activity MarkAsHealthCheck(this Activity activity) =>
			activity.IsHealthCheck()
				? activity
				: activity.SetBaggage(s_healthCheckMarkerKey, s_healthCheckMarkerValue);

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
			Guid.TryParse(activity.GetBaggageItem(s_obsoleteCorrelationId), out Guid correlation)
				? correlation
				: null;

		/// <summary>
		/// Set correlation guid that is used by old Omex services
		/// </summary>
		/// <remarks>This property would be transfered to child activity and via web requests</remarks>
		[Obsolete(CorrelationIdObsoleteMessage, false)]
		public static Activity SetObsoleteCorrelationId(this Activity activity, Guid correlation) =>
			activity.SetBaggage(s_obsoleteCorrelationId, correlation.ToString());

		/// <summary>
		/// Get transaction id that is used by old Omex services
		/// </summary>
		[Obsolete(TransactionIdObsoleteMessage, false)]
		public static uint? GetObsoleteTransactionId(this Activity activity) =>
			uint.TryParse(activity.GetBaggageItem(s_obsoleteTransactionId), out uint transactionId)
				? transactionId
				: null;

		/// <summary>
		/// Set transaction id that is used by old Omex services
		/// </summary>
		/// <remarks>This property would be transfered to child activity and via web requests</remarks>
		[Obsolete(TransactionIdObsoleteMessage, false)]
		public static Activity SetObsoleteTransactionId(this Activity activity, uint transactionId) =>
			activity.SetBaggage(s_obsoleteTransactionId, transactionId.ToString(CultureInfo.InvariantCulture));

		private static readonly string s_userHashKey = "UserHash";
		private static readonly string s_healthCheckMarkerKey = "HealthCheckMarker";
		private static readonly string s_healthCheckMarkerValue = "true";
		private static readonly string s_performanceMarkerKey = "PerformanceTestMarker";
		private static readonly string s_performanceMarkerValue = "true";
		private static readonly string s_obsoleteCorrelationId = "ObsoleteCorrelationId";
		private static readonly string s_obsoleteTransactionId = "ObsoleteTransactionId";
		private const string CorrelationIdObsoleteMessage = "Please use Activity.Id or Activity.GetRootIdAsGuid() for new services instead";
		private const string TransactionIdObsoleteMessage = "Please use Activity.IsHealthCheck() for new services instead";
	}
}
