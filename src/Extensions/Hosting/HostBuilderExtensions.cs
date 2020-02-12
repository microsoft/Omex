// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Compatability;
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
		public static IHostBuilder ConfigureOmexService(this IHostBuilder builder)
		{
			return builder.ConfigureServices(collection =>
			{
				collection
					.AddOmexLogging<NullServiceContext>()
					.AddTimedScopes()
					.AddOmexCompatability();
			});
		}
	}
}
