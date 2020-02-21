using System;
using System.Fabric;
using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web
{
	/// <summary>
	/// Extension to add Omex dependencies to HostBuilder
	/// </summary>
	public static class HostBuilderExtensions
	{
		/// <summary>Add Kestrel service listener to stateless service</summary>
		public static ServiceFabricHostBuilder<StatelessServiceContext> AddKestrelListener<TStartup>(
			this ServiceFabricHostBuilder<StatelessServiceContext> builder,
			string name,
			ServiceFabricIntegrationOptions options,
			Action<IWebHostBuilder>? builderExtension = null)
				where TStartup : class =>
			builder.AddKestrelListener<TStartup, StatelessServiceContext>(name, options, builderExtension);


		/// <summary>Add Kestrel service listener to stateful service</summary>
		public static ServiceFabricHostBuilder<StatefulServiceContext> AddKestrelListener<TStartup>(
			this ServiceFabricHostBuilder<StatefulServiceContext> builder,
			string name,
			ServiceFabricIntegrationOptions options,
			Action<IWebHostBuilder>? builderExtension = null)
				where TStartup : class =>
			builder.AddKestrelListener<TStartup, StatefulServiceContext>(name, options, builderExtension);


		private static ServiceFabricHostBuilder<TServiceContext> AddKestrelListener<TStartup, TServiceContext>(
			this ServiceFabricHostBuilder<TServiceContext> builder,
			string name,
			ServiceFabricIntegrationOptions options,
			Action<IWebHostBuilder>? builderExtension = null)
				where TStartup : class
				where TServiceContext : ServiceContext =>
			builder.AddServiceListener(new KestrelListenerBuilder<TStartup, TServiceContext>(
				name,
				options,
				builder => BuilderExtension(builder, builderExtension)));


		private static void BuilderExtension(IWebHostBuilder builder, Action<IWebHostBuilder>? builderExtension)
		{
			builderExtension?.Invoke(builder);
			builder.ConfigureServices((context, collection) => collection.AddOmexServiceFabricDependencies());
		}
	}
}
