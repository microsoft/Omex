using System;
using System.Fabric;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Hosting;
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
			builder.AddServiceListener(new KestrelListenerBuilder<StatelessServiceContext>(
				typeof(TStartup),
				name,
				options,
				builder => BuilderExtension(builder, builderExtension)));


		private static void BuilderExtension(IWebHostBuilder builder, Action<IWebHostBuilder>? builderExtension)
		{
			builderExtension?.Invoke(builder);
			builder.ConfigureServices((context, collection) => collection.AddOmexServices());
		}
	}
}
