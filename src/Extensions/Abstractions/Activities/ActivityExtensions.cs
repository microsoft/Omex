// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Microsoft.Omex.Extensions.Abstractions.Activities.Processing;

namespace Microsoft.Omex.Extensions.Abstractions.Activities
{
	/// <summary>
	/// Extensions for Activity
	/// </summary>
	public static class ActivityExtensions
	{
		/// <summary>
		/// Transforms RootId into Guid that might be used in places wheere old Correlation Id needed
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
			activity.SetBaggage(HealthCheckMarkerKey, HealthCheckMarkerValue);

		/// <summary>
		/// Set result
		/// </summary>
		/// <remarks>This property won't be transferred to child activity or via web requests</remarks>
		public static Activity SetResult(this Activity activity, TimedScopeResult result) =>
			activity.SetTag(ActivityTagKeys.Result, ActivityResultStrings.ResultToString(result));

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
			activity.SetBaggage(ObsoleteTransactionId, transactionId.ToString(CultureInfo.InvariantCulture));

		private const string UserHashKey = "UserHash";
		private const string HealthCheckMarkerKey = "HealthCheckMarker";
		private const string HealthCheckMarkerValue = "true";
		private const string ObsoleteCorrelationId = "ObsoleteCorrelationId";
		private const string ObsoleteTransactionId = "ObsoleteTransactionId";
		private const string CorrelationIdObsoleteMessage = "Please use Activity.Id for new services instead";
		private const string TransactionIdObsoleteMessage = "Please use Activity.TraceId for new services instead";
	}

	internal static class ActivityHacks
	{
		/// <summary>
		/// Adds or replaces baggage item in the Activity
		/// </summary>
		/// <remarks>
		/// The method created as a temporary replacement until actual SetBaggage will be available, most probably in net6.0 (https://github.com/dotnet/runtime/issues/42706)
		/// It relies heavily on the internal structure of the Activity and might need updates after activity implementation change (if it's still needed in net6.0)
		/// It's named the same as future API method to force compiler error (or automatic usage of proper method) when the actual method available and force deletion of this hack
		/// </remarks>
		internal static Activity SetBaggage(this Activity activity, string key, string value)
		{
			if (activity.GetBaggageItem(key) != null)
			{
				ReplaceBaggageValue(activity, key, value);
			}
			else
			{
				activity.AddBaggage(key, value);
			}

			return activity;
		}

		private static void ReplaceBaggageValue(Activity activity, string key, string value)
		{
			s_baggageField ??= activity.GetType().GetField("_baggage", BindingFlags.NonPublic | BindingFlags.Instance);
			if (s_baggageField == null)
			{
				return;
			}

			object? baggage = s_baggageField.GetValue(activity);

			s_headField ??= s_baggageField.FieldType.GetField("_first", BindingFlags.NonPublic | BindingFlags.Instance);
			if (s_headField == null)
			{
				return;
			}

			object? node = s_headField.GetValue(baggage);
			if (node == null)
			{
				return;
			}

			s_listNodeType ??= node.GetType();
			s_valueField ??= s_listNodeType.GetField("Value");
			s_nextField ??= s_listNodeType.GetField("Next");

			if (s_valueField == null || s_nextField == null)
			{
				return;
			}

			while (true)
			{
				KeyValuePair<string, string?>? pair = s_valueField.GetValue(node) as KeyValuePair<string, string?>?;
				if (pair.HasValue && pair.Value.Key == key)
				{
					break;
				}

				node = s_nextField.GetValue(node);
				if (node == null)
				{
					return;
				}
			}

			s_valueField.SetValue(node, new KeyValuePair<string, string?>(key, value));
		}

		private static Type? s_listNodeType;
		private static FieldInfo? s_baggageField;
		private static FieldInfo? s_headField;
		private static FieldInfo? s_valueField;
		private static FieldInfo? s_nextField;
	}
}
