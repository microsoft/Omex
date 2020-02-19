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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Omex.Extensions.Abstractions;
using Microsoft.Omex.Extensions.Logging;
using Microsoft.Omex.Extensions.ServiceFabric;
using Microsoft.Omex.Extensions.TimedScopes;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

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
					collection.AddServiceFabricDependencies();
				})
				.AddOmexServices();
		}


		/// <summary>Add actions that will be executed inside service RunAsync method</summary>
		public static IHostBuilder AddServiceAction(
			this IHostBuilder builder,
			Func<StatelessService, CancellationToken, Task> action) =>
			builder.ConfigureServices((config, collection) => collection.AddSingleton(new ServiceAction(action)));


		/// <summary>Add service listener to SF listener</summary>
		public static IHostBuilder AddServiceListener(
			this IHostBuilder builder,
			string name,
			Func<StatelessServiceContext, ICommunicationListener> createListener) =>
			builder.ConfigureServices((config, collection) => collection.AddSingleton(new ListenerBuilder(name, createListener)));


		/// <summary>
		/// Configures host to run Service Fabric Stateless service with initializded Omex Dependencies
		/// </summary>
		public static IHost BuildStelessService(this IHostBuilder builder)
		{
			try
			{
				IHost host = builder
					.AddOmexServiceFabricServices()
					.ConfigureServices((context, collection) =>
					{
						collection
							.AddTransient<IHostedService, OmexHostedService>()
							.AddSingleton<OmexServiceRunner, OmexServiceRunner>()
							.AddSingleton<IOmexServiceRunner>(p => p.GetService<OmexServiceRunner>())
							.AddSingleton<IStatelessServiceContextAccessor>(p => p.GetService<OmexServiceRunner>());
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
