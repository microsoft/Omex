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
	/// Class to manage stateful service fabric service startup
	/// </summary>
	internal sealed class OmexStatefulServiceRunner : IOmexServiceRunner, IServiceContextAccessor<StatefulServiceContext>
	{
		private readonly string m_applicationName;
		private IEnumerable<IListenerBuilder<StatefulServiceContext>> ListenerBuilders { get; }
		private IEnumerable<IServiceAction<StatefulServiceContext>> ServiceActions { get; }
		public StatefulServiceContext? ServiceContext { get; private set; }

		public OmexStatefulServiceRunner(
			IHostEnvironment environment,
			IEnumerable<IListenerBuilder<StatefulServiceContext>> listenerBuilders,
			IEnumerable<IServiceAction<StatefulServiceContext>> serviceActions)
		{
			m_applicationName = environment.ApplicationName;
			ListenerBuilders = listenerBuilders;
			ServiceActions = serviceActions;
		}


		public Task RunServiceAsync(CancellationToken cancellationToken) =>
			ServiceRuntime.RegisterServiceAsync(m_applicationName, ServiceFactory, cancellationToken: cancellationToken);


		private StatefulService ServiceFactory(StatefulServiceContext context)
		{
			ServiceContext = context;
			return new OmexStatefulService(this, context);
		}


		private class OmexStatefulService : StatefulService
		{
			private readonly OmexStatefulServiceRunner m_serviceParameters;

			public OmexStatefulService(
				OmexStatefulServiceRunner serviceRunner,
				StatefulServiceContext serviceContext)
					: base(serviceContext)
			{
				m_serviceParameters = serviceRunner;
			}


			protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners() =>
				m_serviceParameters.ListenerBuilders.Select(b => new ServiceReplicaListener(b.Build, b.Name));


			protected override Task RunAsync(CancellationToken cancellationToken) =>
				Task.WhenAll(m_serviceParameters.ServiceActions.Select(r => r.RunAsync(Context, cancellationToken)));
		}
	}
}
