// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Abstractions.Activities.Processing
{
	/// <summary>
	/// Activity does not have methods to extract tags efficiently, so we need to expose keys to provide ability of extracting tags
	/// </summary>
	public static class ActivityTagKeys
	{
		/// <summary>
		/// Activity result tag key
		/// </summary>
		public static string Result { get; } = "Result";


		/// <summary>
		/// Activity sub type tag key
		/// </summary>
		public static string SubType { get; } = "SubType";


		/// <summary>
		/// Activity metadata tag key
		/// </summary>
		public static string Metadata { get; } = "Metadata";
	}
}
