// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;

namespace Microsoft.Omex.Extensions.Abstractions.Activities
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
			activity.GetBaggageItem(UserHashKey) ?? string.Empty;


		/// <summary>
		/// Returns true if activity is transaction
		/// </summary>
		public static bool IsTransaction(this Activity activity) =>
			string.Equals(
				activity.GetBaggageItem(TransactionMarkerKey),
				TransactionMarkerValue,
				StringComparison.OrdinalIgnoreCase);


		/// <summary>
		/// Set user hash for the activity
		/// </summary>
		/// <remarks>This property would be transfered to child activity and via web requests</remarks>
		public static Activity SetUserHash(this Activity activity, string userHash) =>
			activity.AddBaggage(UserHashKey, userHash);


		/// <summary>
		/// Mark activity as transaction
		/// </summary>
		/// <remarks>This property would be transfered to child activity and via web requests</remarks>
		public static Activity MarkAsTransaction(this Activity activity) =>
			activity.AddBaggage(TransactionMarkerKey, TransactionMarkerValue);


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
			activity.AddBaggage(ObsoleteCorrelationId, correlation.ToString());


		/// <summary>
		/// Get transaction id that is used by old Omex services
		/// </summary>
		[Obsolete(TransactionIdObsoleteMessage, false)]
		public static uint? GetObsolteteTransactionId(this Activity activity) =>
			uint.TryParse(activity.GetBaggageItem(ObsoleteTransactionId), out uint transactionId)
				? transactionId
				: (uint?)null;


		/// <summary>
		/// Set transaction id that is used by old Omex services
		/// </summary>
		/// <remarks>This property would be transfered to child activity and via web requests</remarks>
		[Obsolete(TransactionIdObsoleteMessage, false)]
		public static Activity SetObsoleteTransactionId(this Activity activity, uint transactionId) =>
			activity.AddBaggage(ObsoleteTransactionId, transactionId.ToString());


		private const string UserHashKey = "UserHash";
		private const string TransactionMarkerKey = "TransactionMarkerKey";
		private const string TransactionMarkerValue = "true";
		private const string ObsoleteCorrelationId = "ObsoleteCorrelationId";
		private const string ObsoleteTransactionId = "ObsoleteTransactionId";
		private const string CorrelationIdObsoleteMessage = "Please use Activity.Id for new services instead";
		private const string TransactionIdObsoleteMessage = "Please use Activity.TraceId for new services instead";
	}
}
