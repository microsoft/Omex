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


		/// <summary>Add actions that will be executed inside service RunAsync method</summary>
		public static IHostBuilder AddServiceAction(
			this IHostBuilder builder,
			Func<StatelessServiceContext, CancellationToken, Task> action) =>
			builder.AddServiceAction(new ServiceAction<StatelessServiceContext>(action));


		/// <summary>Add actions that will be executed inside service RunAsync method</summary>
		public static IHostBuilder AddServiceAction(
			this IHostBuilder builder,
			IServiceAction<StatelessServiceContext> action) =>
			builder.ConfigureServices((config, collection) => collection.AddSingleton(action));


		/// <summary>Add service listener to SF listener</summary>
		public static IHostBuilder AddServiceListener(
			this IHostBuilder builder,
			string name,
			Func<StatelessServiceContext, ICommunicationListener> createListener) =>
			builder.AddServiceListener(new ListenerBuilder<StatelessServiceContext>(name, createListener));


		/// <summary>Add service listener to SF listener</summary>
		public static IHostBuilder AddServiceListener(
			this IHostBuilder builder,
			IListenerBuilder<StatelessServiceContext> listenerBuilder) =>
			builder.ConfigureServices((config, collection) => collection.AddSingleton(listenerBuilder));


		/// <summary>
		/// Configures host to run Service Fabric Stateless service with initializded Omex Dependencies
		/// </summary>
		public static IHost BuildStelessService(this IHostBuilder builder)
		{
			try
			{
				IHost host = builder
					.ConfigureServices((context, collection) =>
					{
						collection
							.AddTransient<IHostedService, OmexHostedService>()
							.AddSingleton<OmexServiceRunner, OmexServiceRunner>()
							.AddSingleton<IOmexServiceRunner>(p => p.GetService<OmexServiceRunner>())
							.AddSingleton<IServiceContextAccessor<StatelessServiceContext>>(p => p.GetService<OmexServiceRunner>());
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


		/// <summary> Registerin DI classes that will provide Serfice Fabric specific information for logging </summary>
		public static IServiceCollection AddOmexServiceFabricDependencies(this IServiceCollection serviceCollection)
		{
			serviceCollection.TryAddTransient<IServiceContext, OmexServiceFabricContext>();
			serviceCollection.TryAddSingleton<IMachineInformation, ServiceFabricMachineInformation>();
			return serviceCollection.AddOmexServices();
		}
	}
}
