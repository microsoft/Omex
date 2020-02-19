using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Microsoft.Omex.Extensions.Hosting.Services
{
	/// <summary>
	/// Class to manage service startup
	/// </summary>
	internal class OmexServiceRunner : IOmexServiceRunner, IStatelessServiceContextAccessor
	{
		private readonly string m_applicationName;
		private IEnumerable<ListenerBuilder> ListenerBuilders { get; }
		private IEnumerable<ServiceAction> ServiceActions { get; }
		public StatelessServiceContext? ServiceContext { get; private set; }

		public OmexServiceRunner(
			IHostEnvironment environment,
			IEnumerable<ListenerBuilder> listenerBuilders,
			IEnumerable<ServiceAction> serviceActions)
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
