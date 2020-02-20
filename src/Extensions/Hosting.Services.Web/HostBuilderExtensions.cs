using System;
using System.Fabric;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web
{
	/// <summary>
	/// Extension to add Omex dependencies to HostBuilder
	/// </summary>
	public static class HostBuilderExtensions
	{
		/// <summary>Add Kestrel service listener to SF stateless service</summary>
		public static IHostBuilder AddKestrelStatelessListener<TStartup>(
			this IHostBuilder builder,
			string name,
			ServiceFabricIntegrationOptions options,
			Action<IWebHostBuilder>? builderExtension = null) =>
			builder.AddKestrelServiceListener<TStartup, StatelessServiceContext>(name, options, builderExtension);


		/// <summary>Add Kestrel service listener to SF stateful service</summary>
		public static IHostBuilder AddKestrelStatefulListener<TStartup>(
			this IHostBuilder builder,
			string name,
			ServiceFabricIntegrationOptions options,
			Action<IWebHostBuilder>? builderExtension = null) =>
			builder.AddKestrelServiceListener<TStartup, StatefulServiceContext>(name, options, builderExtension);


		private static IHostBuilder AddKestrelServiceListener<TStartup, TServiceContext>(
			this IHostBuilder builder,
			string name,
			ServiceFabricIntegrationOptions options,
			Action<IWebHostBuilder>? builderExtension = null) where TServiceContext : ServiceContext =>
			builder.AddServiceListener(new KestrelListenerBuilder<StatelessServiceContext>(
				typeof(TStartup),
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
