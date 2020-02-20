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
	/// Class to manage stateless service fabric service startup
	/// </summary>
	internal sealed class OmexStatelessServiceRunner : IOmexServiceRunner, IServiceContextAccessor<StatelessServiceContext>
	{
		private readonly string m_applicationName;
		private IEnumerable<IListenerBuilder<StatelessServiceContext>> ListenerBuilders { get; }
		private IEnumerable<IServiceAction<StatelessServiceContext>> ServiceActions { get; }
		public StatelessServiceContext? ServiceContext { get; private set; }

		public OmexStatelessServiceRunner(
			IHostEnvironment environment,
			IEnumerable<IListenerBuilder<StatelessServiceContext>> listenerBuilders,
			IEnumerable<IServiceAction<StatelessServiceContext>> serviceActions)
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
			private readonly OmexStatelessServiceRunner m_serviceParameters;

			public OmexStatelessService(
				OmexStatelessServiceRunner serviceRunner,
				StatelessServiceContext serviceContext)
					: base(serviceContext)
			{
				m_serviceParameters = serviceRunner;
			}


			protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners() =>
				m_serviceParameters.ListenerBuilders.Select(b => new ServiceInstanceListener(b.Build, b.Name));


			protected override Task RunAsync(CancellationToken cancellationToken) =>
				Task.WhenAll(m_serviceParameters.ServiceActions.Select(r => r.RunAsync(Context, cancellationToken)));
		}
	}
}
