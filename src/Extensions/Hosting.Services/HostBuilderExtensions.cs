using System;
using System.Diagnostics;
using System.Fabric;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Extension to add Omex dependencies to HostBuilder
	/// </summary>
	public static class HostBuilderExtensions
	{
		/// <summary>Add actions that will be executed inside stateless service RunAsync method</summary>
		public static IHostBuilder AddServiceAction(
			this IHostBuilder builder,
			Func<StatefulServiceContext, CancellationToken, Task> action) =>
			builder.AddServiceAction(new ServiceAction<StatefulServiceContext>(action));


		/// <summary>Add actions that will be executed inside stateless service RunAsync method</summary>
		public static IHostBuilder AddServiceAction(
			this IHostBuilder builder,
			Func<StatelessServiceContext, CancellationToken, Task> action) =>
			builder.AddServiceAction(new ServiceAction<StatelessServiceContext>(action));


		/// <summary>Add actions that will be executed inside stateless service RunAsync method</summary>
		public static IHostBuilder AddServiceAction(
			this IHostBuilder builder,
			IServiceAction<StatefulServiceContext> action) =>
			builder.ConfigureServices((config, collection) => collection.AddSingleton(action));


		/// <summary>Add actions that will be executed inside stateless service RunAsync method</summary>
		public static IHostBuilder AddServiceAction(
			this IHostBuilder builder,
			IServiceAction<StatelessServiceContext> action) =>
			builder.ConfigureServices((config, collection) => collection.AddSingleton(action));


		/// <summary>Add service listener to stateless service fabric service</summary>
		public static IHostBuilder AddServiceListener(
			this IHostBuilder builder,
			string name,
			Func<StatefulServiceContext, ICommunicationListener> createListener) =>
			builder.AddServiceListener(new ListenerBuilder<StatefulServiceContext>(name, createListener));


		/// <summary>Add service listener to stateless service fabric service</summary>
		public static IHostBuilder AddServiceListener(
			this IHostBuilder builder,
			string name,
			Func<StatelessServiceContext, ICommunicationListener> createListener) =>
			builder.AddServiceListener(new ListenerBuilder<StatelessServiceContext>(name, createListener));


		/// <summary>Add service listener to stateless service fabric service</summary>
		public static IHostBuilder AddServiceListener(
			this IHostBuilder builder,
			IListenerBuilder<StatefulServiceContext> listenerBuilder) =>
			builder.ConfigureServices((config, collection) => collection.AddSingleton(listenerBuilder));


		/// <summary>Add service listener to stateless service fabric service</summary>
		public static IHostBuilder AddServiceListener(
			this IHostBuilder builder,
			IListenerBuilder<StatelessServiceContext> listenerBuilder) =>
			builder.ConfigureServices((config, collection) => collection.AddSingleton(listenerBuilder));


		/// <summary>
		/// Configures host to run service fabric stateless service with initializded Omex dependencies
		/// </summary>
		public static IHost BuildStelessService(this IHostBuilder builder) =>
			builder.BuildServiceFabricService(collection =>
			{
				collection
					.AddSingleton<OmexStatelessServiceRunner, OmexStatelessServiceRunner>()
					.AddSingleton<IOmexServiceRunner>(p => p.GetService<OmexStatelessServiceRunner>())
					.AddSingleton<IServiceContextAccessor<StatelessServiceContext>>(p => p.GetService<OmexStatelessServiceRunner>());
			});


		/// <summary>
		/// Configures host to run service fabric statefull service with initializded Omex dependencies
		/// </summary>
		public static IHost BuildStatefullService(this IHostBuilder builder) =>
			builder.BuildServiceFabricService(collection =>
			{
				collection
					.AddSingleton<OmexStatefulServiceRunner, OmexStatefulServiceRunner>()
					.AddSingleton<IOmexServiceRunner>(p => p.GetService<OmexStatefulServiceRunner>())
					.AddSingleton<IServiceContextAccessor<StatefulServiceContext>>(p => p.GetService<OmexStatefulServiceRunner>());
			});


		/// <summary> Registerin DI classes that will provide Serfice Fabric specific information for logging </summary>
		public static IServiceCollection AddOmexServiceFabricDependencies(this IServiceCollection serviceCollection)
		{
			serviceCollection.TryAddTransient<IServiceContext, OmexServiceFabricContext>();
			serviceCollection.TryAddSingleton<IMachineInformation, ServiceFabricMachineInformation>();
			return serviceCollection.AddOmexServices();
		}


		/// <summary>Add required Omex dependencies</summary>
		public static IHostBuilder AddOmexServiceFabricServices(this IHostBuilder builder)
		{
			return builder
				.ConfigureServices((context, collection) =>
				{
					collection
						.AddOmexServiceFabricDependencies();
				});
		}


		private static IHost BuildServiceFabricService(this IHostBuilder builder, Action<IServiceCollection> registerDependencies)
		{
			try
			{
				IHost host = builder
					.ConfigureServices((context, collection) =>
					{
						registerDependencies(collection);
						collection.AddTransient<IHostedService, OmexHostedService>();
					})
					.AddOmexServiceFabricServices()
					.Build();

				string m_applicationName = Assembly.GetExecutingAssembly().GetName().FullName;
				ServiceInitializationEventSource.Instance.LogServiceTypeRegistered(Process.GetCurrentProcess().Id, m_applicationName);

				return host;
			}
			catch (Exception e)
			{
				ServiceInitializationEventSource.Instance.LogServiceHostInitializationFailed(e.ToString());
				throw;
			}
		}
	}
}
