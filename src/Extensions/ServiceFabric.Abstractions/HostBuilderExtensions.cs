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
							.AddTransient<IOmexServiceRunner, OmexServiceRunner>()
							.AddTransient<IHostedService, OmexHostedService>();
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
	}

	internal interface IOmexServiceRunner
	{
		Task RunServiceAsync(CancellationToken cancellationToken);
	}

	internal interface IServiceAction
	{
		Task RunAsync(CancellationToken cancellationToken);
	}

	internal interface IListenerBuilder
	{
		ServiceInstanceListener Build();
	}

	/// <summary>
	/// Handless service startup
	/// </summary>
	internal class OmexServiceRunner : IOmexServiceRunner
	{
		private readonly string m_applicationName;
		private IEnumerable<IListenerBuilder> ListenerBuilders { get; }
		private IEnumerable<IServiceAction> ServiceActions { get; }


		public OmexServiceRunner(
			IHostEnvironment environment,
			IEnumerable<IListenerBuilder> listenerBuilders,
			IEnumerable<IServiceAction> serviceActions)
		{
			m_applicationName = environment.ApplicationName;
			ListenerBuilders = listenerBuilders;
			ServiceActions = serviceActions;
		}


		public Task RunServiceAsync(CancellationToken cancellationToken)
		{
			return ServiceRuntime.RegisterServiceAsync(m_applicationName,
				context => new OmexStatelessService(this, context));
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
				m_serviceParameters.ListenerBuilders.Select(b => b.Build());


			protected override Task RunAsync(CancellationToken cancellationToken) =>
				Task.WhenAll(m_serviceParameters.ServiceActions.Select(r => r.RunAsync(cancellationToken)));
		}
	}
}
