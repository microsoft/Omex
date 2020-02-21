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
		/// <summary>Add actions that will be executed inside service fabric service RunAsync method</summary>
		public static ServiceFabricHostBuilder<TContext> AddServiceAction<TContext>(
			this ServiceFabricHostBuilder<TContext> builder,
			Func<TContext, CancellationToken, Task> action)
				where TContext : ServiceContext =>
			builder.AddServiceAction(new ServiceAction<TContext>(action));


		/// <summary>Add actions that will be executed inside stateless service RunAsync method</summary>
		public static ServiceFabricHostBuilder<TContext> AddServiceAction<TContext>(
			this ServiceFabricHostBuilder<TContext> builder,
			IServiceAction<TContext> action)
				where TContext : ServiceContext =>
			builder.ConfigureServices((config, collection) => collection.AddSingleton(action));


		/// <summary>Add service listener to stateless service fabric service</summary>
		public static ServiceFabricHostBuilder<TContext> AddServiceListener<TContext>(
			this ServiceFabricHostBuilder<TContext> builder,
			string name,
			Func<TContext, ICommunicationListener> createListener)
				where TContext : ServiceContext =>
			builder.AddServiceListener(new ListenerBuilder<TContext>(name, createListener));


		/// <summary>Add service listener to stateless service fabric service</summary>
		public static ServiceFabricHostBuilder<TContext> AddServiceListener<TContext>(
			this ServiceFabricHostBuilder<TContext> builder,
			IListenerBuilder<TContext> listenerBuilder)
				where TContext : ServiceContext =>
			builder.ConfigureServices((config, collection) => collection.AddSingleton(listenerBuilder));


		/// <summary>Configures host to run service fabric stateless service with initializded Omex dependencies</summary>
		public static IHost BuildStelessService(
			this IHostBuilder builder,
			Action<ServiceFabricHostBuilder<StatelessServiceContext>> builderAction) =>
			builder.BuildServiceFabricService<OmexStatelessServiceRunner, StatelessServiceContext>(builderAction);


		/// <summary>Configures host to run service fabric statefull service with initializded Omex dependencies</summary>
		public static IHost BuildStatefullService(
			this IHostBuilder builder,
			Action<ServiceFabricHostBuilder<StatefulServiceContext>> builderAction) =>
			builder.BuildServiceFabricService<OmexStatefulServiceRunner, StatefulServiceContext>(builderAction);


		/// <summary> Registerin DI classes that will provide Serfice Fabric specific information for logging </summary>
		public static IServiceCollection AddOmexServiceFabricDependencies(this IServiceCollection serviceCollection)
		{
			serviceCollection.TryAddTransient<IServiceContext, OmexServiceFabricContext>();
			serviceCollection.TryAddSingleton<IMachineInformation, ServiceFabricMachineInformation>();
			return serviceCollection.AddOmexServices();
		}


		///// <summary>Add required Omex dependencies</summary>
		//public static IHostBuilder AddOmexServiceFabricServices(this IHostBuilder builder)
		//{
		//	return builder
		//		.ConfigureServices((context, collection) =>
		//		{
		//			collection
		//				.AddOmexServiceFabricDependencies();
		//		});
		//}


		private static IHost BuildServiceFabricService<TRunner,TContext>(
			this IHostBuilder builder,
			Action<ServiceFabricHostBuilder<TContext>> builderAction)
				where TContext : ServiceContext
				where TRunner : class, IOmexServiceRunner, IServiceContextAccessor<TContext>
		{
			try
			{
				builderAction(new ServiceFabricHostBuilder<TContext>(builder));

				IHost host = builder
					.ConfigureServices((context, collection) =>
					{
						collection
							.AddOmexServiceFabricDependencies()
							.AddTransient<IHostedService, OmexHostedService>()
							.AddSingleton<TRunner, TRunner>()
							.AddSingleton<IOmexServiceRunner>(p => p.GetService<TRunner>())
							.AddSingleton<IServiceContextAccessor<TContext>>(p => p.GetService<TRunner>());
					})
					.UseDefaultServiceProvider(options =>
					{
						options.ValidateOnBuild = true;
						options.ValidateScopes = true;
					})
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
