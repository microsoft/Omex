using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.Omex.Extensions.ServiceFabric;
using Microsoft.Omex.Extensions.ServiceFabric.Services;
using Microsoft.Omex.Extensions.TimedScopes;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

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
			Action<IWebHostBuilder>? builderExtension = null)
		{
			builder.AddServiceListener(new KestrelListenerBuilder<StatelessServiceContext>(typeof(TStartup), name, options, builderExtension));
		}
	}
}
