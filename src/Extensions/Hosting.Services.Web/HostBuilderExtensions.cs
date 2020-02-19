using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.ServiceFabric.Services;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web
{
	/// <summary>
	/// Extension to add Omex dependencies to HostBuilder
	/// </summary>
	public static class HostBuilderExtensions
	{
		/// <summary>Add Kestrel service listener to SF service</summary>
		public static IHostBuilder AddKestrelListener<TStartup>(
			this IHostBuilder builder,
			string name,
			ServiceFabricIntegrationOptions options,
			Action<IWebHostBuilder>? builderExtension = null) =>
			builder.AddServiceListener(new KestrelListenerBuilder<StatelessServiceContext>(typeof(TStartup), name, options, builderExtension));
	}
}
