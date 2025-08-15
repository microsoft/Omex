// Copyright (C) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.FeatureManagement.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.FeatureManagement;

/// <summary>
/// Extension methods for <see cref="FeatureFilterEvaluationContext"/>.
/// </summary>
internal static class FeatureFilterEvaluationContextExtensions
{
	private const string Wildcard = "*";

	/// <summary>
	/// Determines whether a filter is active based on the current context.
	/// </summary>
	/// <typeparam name="TSettings">The settings type.</typeparam>
	/// <param name="context">The feature filter evaluation context.</param>
	/// <param name="httpContextAccessor">The HTTP context accessor.</param>
	/// <param name="queryParameter">The query-string parameter whose value should be checked.</param>
	/// <param name="extractSettingValue">The function to retrieve the list of values from the setting.</param>
	/// <returns><c>true</c> if the feature is active.</returns>
	public static bool Evaluate<TSettings>(
		this FeatureFilterEvaluationContext context,
		IHttpContextAccessor httpContextAccessor,
		string queryParameter,
		Func<TSettings, List<string>> extractSettingValue)
		where TSettings : new()
	{
		string parameterValue = httpContextAccessor.GetParameter(queryParameter);
		return context.Evaluate<TSettings>(parameterValue, extractSettingValue);
	}

	/// <summary>
	/// Determines whether a filter is active based on the current context.
	/// </summary>
	/// <typeparam name="TSettings">The settings type.</typeparam>
	/// <param name="context">The feature filter evaluation context.</param>
	/// <param name="value">The value that should be checked.</param>
	/// <param name="extractSettingValue">The function to retrieve the list of values from the setting.</param>
	/// <returns><c>true</c> if the feature is active.</returns>
	public static bool Evaluate<TSettings>(
		this FeatureFilterEvaluationContext context,
		string value,
		Func<TSettings, List<string>> extractSettingValue)
		where TSettings : new()
	{
		TSettings filterSettings = context.Parameters.GetOrCreate<TSettings>();
		List<string> list = extractSettingValue(filterSettings);
		if (list.Count == 1 && string.Equals(list[0], Wildcard, StringComparison.Ordinal))
		{
			return true;
		}

		return list.Any(element => string.Equals(element, value, StringComparison.OrdinalIgnoreCase));
	}
}
