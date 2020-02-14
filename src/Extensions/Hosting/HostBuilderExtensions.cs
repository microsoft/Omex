// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.Omex.Extensions.TimedScopes;

namespace Microsoft.Omex.Extensions.ServiceFabric
{
	/// <summary>
	/// Extension to add Omex dependencies to HostBuilder
	/// </summary>
	public static class HostBuilderExtensions
	{
		/// <summary>Add required Omex dependencies</summary>
		public static IHostBuilder AddOmexServices(this IHostBuilder builder)
		{
			return builder
				.ConfigureServices((context, collection) =>
				{
					collection
						.AddOmexLogging()
						.AddTimedScopes();
				});
		}
	}
}
