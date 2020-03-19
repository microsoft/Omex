﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.Omex.Extensions.Abstractions.Activities.Processing
{
	/// <summary>
	/// Class with activity result enum string values to avoid alocations
	/// </summary>
	public static class ActivityResultStrings
	{
		/// <summary>
		/// Activity SystemError result string
		/// </summary>
		public static string SystemError { get; } = "SystemError";

		/// <summary>
		/// Activity ExpectedError result string
		/// </summary>
		public static string ExpectedError { get; } = "ExpectedError";

		/// <summary>
		/// Activity Success result string
		/// </summary>
		public static string Success { get; } = "Success";

		/// <summary>
		/// Activity Unknown result string
		/// </summary>
		public static string Unknown { get; } = "Unknown";

		/// <summary>
		/// Returns corresponding to enum string value with creating new string
		/// </summary>
		public static string ResultToString(TimedScopeResult result) =>
			result switch
			{
				TimedScopeResult.SystemError => SystemError,
				TimedScopeResult.ExpectedError => ExpectedError,
				TimedScopeResult.Success => Success,
				_ => throw new ArgumentException(FormattableString.Invariant($"Unsupported enum value '{result}'"))
			};
	}
}
