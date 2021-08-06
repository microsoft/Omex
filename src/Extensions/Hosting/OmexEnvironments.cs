// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Hosting;

namespace Microsoft.Omex.Extensions.Hosting
{
	/// <summary>
	/// Provides variables with commonly used environments
	/// </summary>
	public static class OmexEnvironments
	{
		/// <summary>
		/// Development environment, previously called Pr
		/// </summary>
		public static string Development { get; } = Environments.Development;

		/// <summary>
		/// Integration environment
		/// This is our CI/CD environment, deployments are triggered on completion of a master build
		/// Consequently, we deploy to INT on completion of a PR
		/// </summary>
		public static string Int { get; } = "Int"; //TODO: Consider Removing GitHub Issue #167

		/// <summary>
		/// Pre production environment
		/// This environment is used to test builds that will eventually make their way to Production
		/// </summary>
		public static string EDog { get; } = "EDog"; //TODO: Consider replacing with Environments.Staging GitHub Issue #167

		/// <summary>
		/// Production/live environment
		/// </summary>
		public static string Production { get; } = Environments.Production;
	}
}
