// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Hosting;

namespace Microsoft.Omex.Extensions.Hosting
{
	/// <summary>
	/// Provides variables with commonly used environments
	/// </summary>
	public static class OmexEnviroments
	{
		/// <summary>
		/// Development environment, previously called Pr
		/// </summary>
		public static string Development { get; } = Environments.Development;


		/// <summary>
		/// Int environment
		/// </summary>
		public static string Int { get; } = "Int"; //TODO: Consider Removing


		/// <summary>
		/// Pre production environment
		/// </summary>
		public static string EDog { get; } = "EDog"; //TODO: Consider replacing with Environments.Staging


		/// <summary>
		/// Production environment
		/// </summary>
		public static string Production { get; } = Environments.Production;
	}
}
