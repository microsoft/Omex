// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

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
	internal sealed class OmexStatefulServiceRunner : IOmexServiceRunner
	{
		private readonly string m_applicationName;
		private readonly ServiceContextAccessor<StatefulServiceContext> m_contextAccessor;
		private IEnumerable<IListenerBuilder<StatefulServiceContext>> ListenerBuilders { get; }
		private IEnumerable<IServiceAction<StatefulServiceContext>> ServiceActions { get; }


		public OmexStatefulServiceRunner(
			IHostEnvironment environment,
			ServiceContextAccessor<StatefulServiceContext> contextAccessor,
			IEnumerable<IListenerBuilder<StatefulServiceContext>> listenerBuilders,
			IEnumerable<IServiceAction<StatefulServiceContext>> serviceActions)
		{
			m_applicationName = environment.ApplicationName;
			m_contextAccessor = contextAccessor;
			ListenerBuilders = listenerBuilders;
			ServiceActions = serviceActions;
		}


		public Task RunServiceAsync(CancellationToken cancellationToken) =>
			ServiceRuntime.RegisterServiceAsync(m_applicationName, ServiceFactory, cancellationToken: cancellationToken);


		private StatefulService ServiceFactory(StatefulServiceContext context)
		{
			m_contextAccessor.SetContext(context);
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
