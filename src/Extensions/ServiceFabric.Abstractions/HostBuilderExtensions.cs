using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
		public static IHostBuilder AddOmexServices(this IHostBuilder builder)
		{
			return builder
				.ConfigureServices(collection =>
				{
					collection
						.AddServiceFabricDependencies()
						.AddOmexLogging()
						.AddTimedScopes();
				});
		}

		/// <summary>
		/// Configures host to run Service Fabric Stateless service with initializded Omex Dependencies
		/// </summary>
		public static IHost BuildStelessService(this IHostBuilder builder)
		{
			try
			{
				IHost host = builder
					.AddOmexServices()
					.ConfigureServices(collection =>
					{
						collection
							.AddTransient<IHostedService, OmexHostedService>()
							.AddSingleton<OmexServiceRunner, OmexServiceRunner>()
							.AddSingleton<IOmexServiceRunner>(p => p.GetService<OmexServiceRunner>())
							.AddSingleton<IStatelessServiceContextAccessor>(p => p.GetService<OmexServiceRunner>());
					}).Build();

				string m_applicationName = Assembly.GetExecutingAssembly().FullName;
				ServiceInitializationEventSource.Instance.LogServiceTypeRegistered(Process.GetCurrentProcess().Id, m_applicationName);

				return host;
			}
			catch (Exception e)
			{
				ServiceInitializationEventSource.Instance.LogServiceHostInitializationFailed(e.ToString());
				throw;
			}
		}


		/// <summary>Add actions that will be executed inside service RunAsync method</summary>
		public static IHost AddServiceAction(this IHostBuilder builder)
		{
			throw new NotImplementedException();
		}


		/// <summary>Add service listener to SF listener</summary>
		public static IHost AddServiceListener(this IHostBuilder builder)
		{
			throw new NotImplementedException();
		}
	}

	internal interface IOmexServiceRunner
	{
		Task RunServiceAsync(CancellationToken cancellationToken);
	}

	internal interface IStatelessServiceContextAccessor
	{
		StatelessServiceContext? ServiceContext { get; }
	}

	internal interface IServiceAction
	{
		Task RunAsync(StatelessService service, CancellationToken cancellationToken);
	}

	internal interface IListenerBuilder
	{
		string Name { get; }

		ICommunicationListener Build(StatelessServiceContext context);
	}

	/// <summary>
	/// Handless service startup
	/// </summary>
	internal class OmexServiceRunner : IOmexServiceRunner, IStatelessServiceContextAccessor
	{
		private readonly string m_applicationName;
		private IEnumerable<IListenerBuilder> ListenerBuilders { get; }
		private IEnumerable<IServiceAction> ServiceActions { get; }
		public StatelessServiceContext? ServiceContext { get; private set; }

		public OmexServiceRunner(
			IHostEnvironment environment,
			IEnumerable<IListenerBuilder> listenerBuilders,
			IEnumerable<IServiceAction> serviceActions)
		{
			m_applicationName = environment.ApplicationName;
			ListenerBuilders = listenerBuilders;
			ServiceActions = serviceActions;
		}


		public Task RunServiceAsync(CancellationToken cancellationToken) =>
			ServiceRuntime.RegisterServiceAsync(m_applicationName, ServiceFactory, cancellationToken: cancellationToken);


		private StatelessService ServiceFactory(StatelessServiceContext context)
		{
			ServiceContext = context;
			return new OmexStatelessService(this, context);
		}


		private class OmexStatelessService : StatelessService
		{
			private readonly OmexServiceRunner m_serviceParameters;

			public OmexStatelessService(
				OmexServiceRunner serviceRunner,
				StatelessServiceContext serviceContext)
					: base(serviceContext)
			{
				m_serviceParameters = serviceRunner;
			}


			protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners() =>
				m_serviceParameters.ListenerBuilders.Select(b => new ServiceInstanceListener(b.Build, b.Name));


			protected override Task RunAsync(CancellationToken cancellationToken) =>
				Task.WhenAll(m_serviceParameters.ServiceActions.Select(r => r.RunAsync(this, cancellationToken)));
		}
	}
}
