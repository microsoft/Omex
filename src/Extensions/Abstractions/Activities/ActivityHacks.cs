// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Microsoft.Omex.Extensions.Abstractions.Activities
{
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
