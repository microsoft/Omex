// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.Omex.Extensions.Abstractions
{
	/// <summary>
	/// Provides variables with commonly used environments
	/// </summary>
	public static class Enviroments
	{
		/// <summary>Development environment, previously called Pr</summary>
		public static string Development { get; } = "Development";


		/// <summary>Int environment</summary>
		public static string Int { get; } = "Int";


		/// <summary>Pre production environment</summary>
		public static string EDog { get; } = "EDog";


		/// <summary>Production environment</summary>
		public static string Production { get; } = "Production";
	}
}
